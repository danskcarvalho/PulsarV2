using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Polly;
using Pulsar.BuildingBlocks.DDD.Abstractions;
using Pulsar.BuildingBlocks.EventBus.Abstractions;
using Pulsar.BuildingBlocks.Sync.Contracts;
using System.Reflection;
using static Pulsar.BuildingBlocks.Utils.GeneralExtensions;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Pulsar.BuildingBlocks.Sync.Services;

public partial class SyncIntegrationEventDispatcher
{
    class Consumer
    {
        private readonly IProducer _producer;
        private readonly IMongoDatabase _database;
        private readonly ILogger _logger;
        private readonly ISaveIntegrationEventLog _eventLog;
        private Assembly[] _assemblies;
        private readonly Dictionary<string, ConsumerForModel> _aggregateRoots = new Dictionary<string, ConsumerForModel>();

        public Consumer(IProducer producer, IMongoDatabase database, Assembly[] assemblies, ILogger logger, ISaveIntegrationEventLog eventLog)
        {
            _producer = producer;
            _database = database;
            _logger = logger;
            _eventLog = eventLog;
            _assemblies = assemblies;
        }

        public async Task Run(CancellationToken ct)
        {
            await Task.Factory.StartNew(async () =>
            {
                int timeout = 1000;
                while (true)
                {
                    if (ct.IsCancellationRequested)
                    {
                        _logger.LogInformation("change consumer cancelled");
                        break;
                    }
                    try
                    {
                        await PopAndRunEvent(ct);
                        timeout = 1000;
                    }
                    catch (TaskCanceledException)
                    {
                        _logger.LogInformation("change consumer cancelled through exception");
                        if (ct.IsCancellationRequested)
                            break;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "error while consuming change events");
                        _logger.LogInformation($"consuming changes will pause for {timeout} milliseconds");
                        await Task.Delay(timeout);
                        timeout *= 2;
                        timeout = Math.Min(timeout, 5 * 60 * 1000); // --> max of 5 minutes
                    }
                }
            }, TaskCreationOptions.LongRunning);
        }

        private async Task PopAndRunEvent(CancellationToken ct)
        {
            var changeEvent = await _producer.Pop(ct);
            var consumer = GetAggregateRootConsumer(changeEvent);
            await consumer.DispatchChangeEvent(changeEvent, ct);
        }

        private ConsumerForModel GetAggregateRootConsumer(ChangeEvent change)
        {
            ScanAggregateRoots();

            lock (_aggregateRoots)
            {
                return _aggregateRoots[change.CollectionName];
            }
        }

        private void ScanAggregateRoots()
        {
            lock (_aggregateRoots)
            {
                if (_aggregateRoots.Count != 0)
                {
                    return;
                }

                foreach (var assembly in _assemblies)
                {
                    var types = assembly.GetTypes().Where(t => t.GetCustomAttribute<TrackChangesAttribute>(false) != null && t.IsClass && typeof(IAggregateRoot).IsAssignableFrom(t));
                    foreach (var type in types)
                    {
                        var attr = type.GetCustomAttribute<TrackChangesAttribute>(false)!;
                        _aggregateRoots[attr.CollectionName ?? throw new InvalidOperationException("no collection name")] = CreateConsumerForModel(_database, _logger, _eventLog, type);
                    }
                }
            }
        }

        private ConsumerForModel CreateConsumerForModel(IMongoDatabase database, ILogger logger, ISaveIntegrationEventLog eventLog, Type type)
        {
            return (ConsumerForModel)(Activator.CreateInstance(
                    typeof(Consumer<>).MakeGenericType(type),
                    _database, _logger, _eventLog
                ) ?? throw new InvalidOperationException($"could not create consumer for type {type}"));
        }
    }

    abstract class ConsumerForModel
    {
        public abstract Task DispatchChangeEvent(ChangeEvent changeEvent, CancellationToken ct);
    }

    class Consumer<TModel> : ConsumerForModel where TModel : class, IAggregateRoot
    {
        private readonly IMongoCollection<TModel> _collection;
        private readonly ILogger _logger;
        private readonly ISaveIntegrationEventLog _eventLog;
        private readonly SourceTypeMetadata _sourceTypeMetadata;

        public Consumer(IMongoDatabase database, ILogger logger, ISaveIntegrationEventLog eventLog)
        {
            var attr = typeof(TModel).GetCustomAttribute<TrackChangesAttribute>(false)!;
            _collection = database.GetCollection<TModel>(attr.CollectionName).WithReadConcern(ReadConcern.Majority).WithReadPreference(ReadPreference.Primary);
            _logger = logger;
            _eventLog = eventLog;
            _sourceTypeMetadata = new SourceTypeMetadata(typeof(TModel));
        }

        private HashSet<string> GetTrackedProps(Type ty)
        {
            return ty.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.GetCustomAttribute<TrackChangesAttribute>() != null).Select(p => p.Name).ToHashSet();
        }

        public override async Task DispatchChangeEvent(ChangeEvent changeEvent, CancellationToken ct)
        {
            var retryPolicy = Policy
                    .Handle<Exception>(e => true)
                    .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt) * 0.5 + Math.Pow(2, retryAttempt) * Random.Shared.NextDouble()));

            await retryPolicy.ExecuteAsync(async (ct2) =>
            {
                try
                {
                    var model = (TModel?)changeEvent.Model;
                    if (model == null && changeEvent.EventKey != ChangedEventKey.Deleted)
                    {
                        var filterDefinition = Builders<TModel>.Filter.Eq("_id", changeEvent.Id);
                        model = await (await _collection.FindAsync(filterDefinition, cancellationToken: ct2)).FirstOrDefaultAsync();
                        if (model == null)
                        {
                            return;
                        }
                    }

                    if (changeEvent.EventKey == ChangedEventKey.Updated && model != null && !AnyPropertiesTracked(model, changeEvent.ChangedProperties))
                    {
                        return;
                    }

                    if (changeEvent.EventKey == ChangedEventKey.Deleted)
                    {
                        model = null;
                    }

                    var shadow = CreateShadow(model, out var entityName);
                    var json = ToJson(shadow);
                    var evt = new EntityChangedIE
                    {
                        ChangeTimestamp = changeEvent.When,
                        ShadowJson = json,
                        ShadowName = entityName,
                        ChangedEntityId = changeEvent.Id,
                        EventKey = changeEvent.EventKey
                    };
                    await _eventLog.SaveEventAsync(evt);
                    await TryResetSyncPendingFlag(model);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "error while watching for change events");
                    throw;
                }
            }, ct);
        }

        private async Task TryResetSyncPendingFlag(TModel? model)
        {
            if (model == null)
            {
                return;
            }
            try
            {
                var updateDefinition = Builders<TModel>.Update
                    .Set(x => x.SyncPendingKey, null)
                    .Set(x => x.IsSyncPending, false);
                var filterDefinition = Builders<TModel>.Filter.And(
                    Builders<TModel>.Filter.Eq("_id", model.Id),
                    Builders<TModel>.Filter.Eq(x => x.Version, model.Version));

                await _collection.UpdateOneAsync(filterDefinition, updateDefinition);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "error while trying to reset sync pending flag");
            }
        }

        private string? ToJson(object? shadow)
        {
            if (shadow == null)
            {
                return null;
            }
            var options = new JsonSerializerOptions();
            options.Converters.Add(new ObjectIdConverter());
            options.Converters.Add(new JsonStringEnumConverter());
            return JsonSerializer.Serialize(shadow, shadow.GetType(), options);
        }

        private object? CreateShadow(TModel? model, out string entityName)
        {
            entityName = _sourceTypeMetadata.ShadowEntityName;
            return model != null ? _sourceTypeMetadata.ToShadow(model) : null;
        }

        private bool AnyPropertiesTracked(TModel model, List<string>? changedProperties)
        {
            if (changedProperties == null)
            {
                // we don't know so we return true
                return true;
            }
            var trackedProps = GetTrackedProps(model.GetType());
            trackedProps.IntersectWith(changedProperties);
            return trackedProps.Any();
        }
    }
}
