﻿using Microsoft.Extensions.Logging;
using Pulsar.BuildingBlocks.DDD.Abstractions;
using Pulsar.BuildingBlocks.EventBus.Abstractions;
using Pulsar.BuildingBlocks.EventBus;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Bson.Serialization;
using static System.Net.Mime.MediaTypeNames;
using Pulsar.BuildingBlocks.Sync.Contracts;
using System.Reflection;
using DDD.Contracts;

namespace Pulsar.BuildingBlocks.Sync.Services;

public partial class SyncIntegrationEventDispatcher
{
    class Producer<TModel> : IProducer where TModel : class, IAggregateRoot
    {
        private const int POLLING_INTERVAL_IN_MS = 60_000;
        public const int MAX_POLLED_ENTITIES = 2000;
        
        private Queue<ChangeEvent> _Queue = new Queue<ChangeEvent>();
        private ILogger _Logger;
        private IMongoCollection<TModel> _Collection;
        private string? _ResumeToken = null;
        private string _CollectionName;

        public Producer(
            IMongoDatabase database,
            ILogger logger)
        {
            this._CollectionName = GetCollectionName<TModel>();
            this._Collection = database.GetCollection<TModel>(_CollectionName);
            this._Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private string GetCollectionName<T>()
        {
            return typeof(T).GetCustomAttribute<TrackChangesAttribute>()?.CollectionName ?? throw new InvalidOperationException($"no TrackChangesAttribute on {typeof(T).Name}");
        }

        public async Task Run(CancellationToken ct)
        {
            var t1 = WatchChanges(ct);
            var t2 = PollChanges(ct);
            await Task.WhenAll(t1, t2);
        }

        public async Task<ChangeEvent> Pop(CancellationToken ct)
        {
            bool delay = false;
            while (true)
            {
                if (ct.IsCancellationRequested)
                    throw new TaskCanceledException();
                if (delay)
                {
                    delay = false;
                    await Task.Delay(20);
                }
                lock (_Queue)
                {
                    if (_Queue.Count == 0)
                    {
                        delay = true;
                        continue;
                    }

                    var evts = _Queue.Dequeue();
                    return evts;
                }
            }
        }

        public ChangeEvent? TryPop()
        {

            lock (_Queue)
            {
                if (_Queue.Count == 0)
                {
                    return null;
                }

                var evts = _Queue.Dequeue();
                return evts;
            }
        }

        public async Task TryPop(CancellationToken ct, Action<Func<ChangeEvent>> popFunction)
        {

            bool delay = false;
            while (true)
            {
                if (ct.IsCancellationRequested)
                    throw new TaskCanceledException();
                if (delay)
                {
                    delay = false;
                    await Task.Delay(20);
                }
                lock (_Queue)
                {
                    if (_Queue.Count == 0)
                    {
                        delay = true;
                        continue;
                    }

                    popFunction(_Queue.Dequeue);
                    return;
                }
            }
        }

        #region [ Main Jobs ]
        private async Task WatchChanges(CancellationToken ct)
        {
            await Task.Factory.StartNew(async () =>
            {
                int timeout = 1000;
                while (true)
                {
                    if (ct.IsCancellationRequested)
                    {
                        _Logger.LogInformation("watching cancelled");
                        break;
                    }
                    try
                    {

                        try
                        {
                            var pipeline =
                              new EmptyPipelineDefinition<ChangeStreamDocument<TModel>>()
                              .Match(x =>
                                  x.OperationType == ChangeStreamOperationType.Insert ||
                                  x.OperationType == ChangeStreamOperationType.Update ||
                                  x.OperationType == ChangeStreamOperationType.Delete ||
                                  x.OperationType == ChangeStreamOperationType.Replace);

                            var options = new ChangeStreamOptions()
                            {
                                FullDocument = ChangeStreamFullDocumentOption.UpdateLookup
                            };

                            var startAfter = await GetResumeToken();
                            if (startAfter != null)
                            {
                                options.StartAfter = startAfter;
                            }

                            using (var cursor = await _Collection.WatchAsync(pipeline, options))
                            {
                                await cursor.ForEachAsync(change =>
                                {
                                    if (ct.IsCancellationRequested)
                                        return;

                                    StoreResumeToken(change.ResumeToken);
                                    BatchEvent(change);
                                }, ct);
                            }
                        }
                        finally
                        {
                            await ForceStoreResumeToken();
                        }
                    }
                    catch (TaskCanceledException)
                    {
                        _Logger.LogInformation("watching cancelled through exception");
                        if (ct.IsCancellationRequested)
                            break;
                    }
                    catch (Exception ex)
                    {
                        _Logger.LogError(ex, "error while watching for change events");
                        _Logger.LogInformation($"watching will pause for {timeout} milliseconds");
                        await Task.Delay(timeout);
                        timeout *= 2;
                        timeout = Math.Min(timeout, 5 * 60 * 1000); // --> max of 5 minutes
                    }
                }
            }, TaskCreationOptions.LongRunning);
        }
        
        private async Task PollChanges(CancellationToken ct)
        {
            await Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    if (ct.IsCancellationRequested)
                    {
                        _Logger.LogInformation("polling cancelled");
                        break;
                    }
                    try
                    {
                        await Task.Delay(POLLING_INTERVAL_IN_MS, ct);

                        while (true)
                        {
                            if (ct.IsCancellationRequested)
                                break;
                            
                            _Logger.LogInformation($"about to poll up to {MAX_POLLED_ENTITIES} events");
                            var entities = (await RetrievePendingEntitiesAsync(MAX_POLLED_ENTITIES, ct)).ToList();
                            if (entities.Count == 0)
                                break;

                            _Logger.LogInformation($"polled {entities.Count} entities");
                            HashSet<ChangeEvent> enqueuedEvents = new HashSet<ChangeEvent>();
                            BatchEntities(entities, enqueuedEvents);

                            // -- we wait the queue to be empty
                            await QueueWasProcessed(enqueuedEvents, ct);
                        }
                    }
                    catch (TaskCanceledException)
                    {
                        _Logger.LogInformation("polling cancelled through exception");
                        if (ct.IsCancellationRequested)
                            break;
                    }
                    catch (Exception ex)
                    {
                        _Logger.LogError(ex, "error while pooling for events");
                    }
                }
            }, TaskCreationOptions.LongRunning);
        }
        #endregion

        #region [ Helpers ]
        private async Task QueueWasProcessed(HashSet<ChangeEvent> enqueuedModels, CancellationToken ct)
        {
            bool delay = false;
            while (true)
            {
                if (ct.IsCancellationRequested)
                    throw new TaskCanceledException();
                if (delay)
                {
                    delay = false;
                    await Task.Delay(20);
                }
                lock (_Queue)
                {
                    var copy = new HashSet<ChangeEvent>(enqueuedModels);
                    copy.IntersectWith(_Queue);

                    if (copy.Count != 0)
                    {
                        delay = true;
                        continue;
                    }

                    return;
                }
            }
        }
        private void BatchEntities(IEnumerable<TModel> entities, HashSet<ChangeEvent> enqueuedEvents)
        {
            lock (_Queue)
            {
                foreach (var entity in entities)
                {
                    var evt = new ChangeEvent(_CollectionName, entity.Id,
                        entity, null, DateTime.UtcNow, GetOperationType(entity.SyncPendingKey!.Value));
                    _Queue.Enqueue(evt);
                    enqueuedEvents.Add(evt);
                }
            }
        }

        private async Task<List<TModel>> RetrievePendingEntitiesAsync(int maxPolledEntities, CancellationToken ct)
        {
            return await (await _Collection.FindAsync(m => m.IsSyncPending && m.SyncPendingKey != null, new FindOptions<TModel>()
            {
                Limit = maxPolledEntities
            })).ToListAsync();
        }
        
        private void BatchEvent(ChangeStreamDocument<TModel> change)
        {
            lock (_Queue)
            {
                if (IsValidEvent(change))
                {
                    _Queue.Enqueue(new ChangeEvent(_CollectionName, change.DocumentKey["_id"].AsObjectId,
                      change.FullDocument, GetChangedProperties(change), change.WallTime ?? DateTime.UtcNow, GetOperationType(change.OperationType)));
                }
            }
        }

        private bool IsValidEvent(ChangeStreamDocument<TModel> change)
        {
            switch (change.OperationType)
            {
                case ChangeStreamOperationType.Update:
                case ChangeStreamOperationType.Delete:
                case ChangeStreamOperationType.Insert:
                case ChangeStreamOperationType.Replace:
                    return true;
                default:
                    return false;
            }
        }

        private ChangedEventKey GetOperationType(ChangeStreamOperationType operationType)
        {
            switch (operationType)
            {
                case ChangeStreamOperationType.Update:
                    return ChangedEventKey.Updated;
                case ChangeStreamOperationType.Delete:
                    return ChangedEventKey.Deleted;
                case ChangeStreamOperationType.Insert:
                    return ChangedEventKey.Inserted;
                case ChangeStreamOperationType.Replace:
                    return ChangedEventKey.Replaced;
                default:
                    throw new InvalidOperationException();
            }
        }
        
        private ChangedEventKey GetOperationType(SyncPendingKey operationType)
        {
            switch (operationType)
            {
                case SyncPendingKey.Updated:
                    return ChangedEventKey.Updated;
                case SyncPendingKey.Inserted:
                    return ChangedEventKey.Inserted;
                case SyncPendingKey.Replaced:
                    return ChangedEventKey.Replaced;
                default:
                    throw new InvalidOperationException();
            }
        }

        private List<string> GetChangedProperties(ChangeStreamDocument<TModel> change)
        {
            List<string> properties = new List<string>();
            if (change.UpdateDescription != null && change.UpdateDescription.UpdatedFields != null)
            {
                foreach (var field in change.UpdateDescription.UpdatedFields)
                {
                    var f = field.Name.Split('.', StringSplitOptions.RemoveEmptyEntries);
                    if (f.Length > 0)
                    {
                        properties.Add(f[0]);
                    }
                }
            }
            return properties;
        }

        private async Task ForceStoreResumeToken()
        {
            try
            {
                if (_ResumeToken == null)
                    return;

                await File.WriteAllTextAsync($".{_CollectionName}.ResumeToken", _ResumeToken);
            }
            catch (Exception e)
            {
                _Logger.LogError(e, "error when storing resume token");
            }
        }

        private void StoreResumeToken(BsonDocument resumeToken)
        {
            try
            {
                _ResumeToken = resumeToken.ToJson();
            }
            catch (Exception e)
            {
                _Logger.LogError(e, $"error when transforming token {resumeToken.ToString()} to json");
            }
        }
        private async Task<BsonDocument?> GetResumeToken()
        {
            try
            {
                if(!File.Exists($".{_CollectionName}.ResumeToken"))
                {
                    return null;
                }
                var file = await File.ReadAllTextAsync($".{_CollectionName}.ResumeToken");
                return BsonSerializer.Deserialize<BsonDocument>(file);
            }
            catch (Exception e)
            {
                _Logger.LogError(e, "error when retrieving resume token");
                return null;
            }
        }
        #endregion
    }

    interface IProducer
    {
        Task<ChangeEvent> Pop(CancellationToken ct);
        ChangeEvent? TryPop();
        Task TryPop(CancellationToken ct, Action<Func<ChangeEvent>> popFunction);
        Task Run(CancellationToken ct);
    }
}
