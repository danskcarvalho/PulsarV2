using System.Reflection;
using MediatR;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Pulsar.BuildingBlocks.DDD;
using Pulsar.BuildingBlocks.DDD.Abstractions;
using Pulsar.BuildingBlocks.DDD.Contexts;
using Pulsar.BuildingBlocks.Sync.Contracts;
using Pulsar.BuildingBlocks.Sync.Domain;
using Pulsar.BuildingBlocks.Sync.Functions.Abstractions;
using Pulsar.BuildingBlocks.Sync.Functions.Entities;
using Pulsar.BuildingBlocks.Utils;

namespace Pulsar.BuildingBlocks.Sync.Functions.Implementations;

public class BatchActivity(
    IDbSession session,
    IBatchManagerFactory batchManagerFactory,
    IEnumerable<IIsRepository> repositories,
    ISyncBatchRepository syncBatchRepository,
    ISyncDbContextFactory factory,
    IMediator mediator,
    ILogger<BatchActivity> logger) : IBatchActivity
{
    private const int MAX_RETRIES_FOR_SHADOW_UPDATE = 5;
    private static readonly Dictionary<string, Type> _shadowTypes = new Dictionary<string, Type>();
    private Type GetShadowTypeFromName(string name)
    {
        lock (_shadowTypes)
        {
            if (_shadowTypes.Count == 0)
            {
                var tys = ShadowAttribute.GetShadowTypes(AppDomain.CurrentDomain.GetAssemblies());
                foreach (var typeAndAttr in tys)
                {
                    _shadowTypes[typeAndAttr.Attribute.Name] = typeAndAttr.Type;
                }
            }

            return _shadowTypes[name];
        }
    }
    
    public Task Execute(BatchActivityDescription desc)
    {
        if (desc is ExecuteBatchActivityDescription eb)
        {
            return ExecuteBatch(eb);
        }
        else if (desc is ExecuteNotificationActivityDescription n)
        {
            return ExecuteNotification(n);
        }
        else
        {
            throw new InvalidOperationException();
        }
    }

    private async Task ExecuteNotification(ExecuteNotificationActivityDescription desc)
    {
        var shadowType = GetShadowTypeFromName(desc.Event.ShadowName);
        var shadow = desc.Event.ShadowJson.FromJsonString(shadowType);
        var trackerAssembly = AppDomain.CurrentDomain.GetAssemblies().First(a => a.FullName == desc.TrackerAssembly);
        var trackerType = trackerAssembly.GetType(desc.TrackerType) ?? throw new InvalidOperationException("no tracker type");
        var rule = trackerType.GetField(desc.RuleName,
            BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic) ?? 
                   throw new InvalidOperationException($"no field {desc.RuleName}");

        var action = (TrackerAction)(rule.GetValue(null) ?? throw new InvalidOperationException());
        if (action.SendNotification == null)
        {
            return;
        }

        try
        {
            var notification = action.SendNotification(shadow!);

            if (notification == null)
            {
                return;
            }
            
            await mediator.Send(notification);
        }
        catch(Exception e)
        {
            logger.LogError(e, $"error when executing notification for shadow {shadowType.FullName} and rule {rule.Name}");
        }
    }

    private async Task ExecuteBatch(ExecuteBatchActivityDescription eb)
    {
        await session.TrackAggregateRoots(async ct =>
        {
            return await session.RetryOnExceptions(
                async _ =>
                {
                    var batchManager = new BatchManagerForDatabase(factory);
                    var batch = await batchManager.GetFromId(eb.BatchId);
                    await batch.Execute();
            
                    return Unit.Value;
                },
                [typeof(VersionConcurrencyException)], 
                MAX_RETRIES_FOR_SHADOW_UPDATE);
        });
    }

    public async Task<TResult> Execute<TResult>(BatchActivityDescription<TResult> desc)
    {
        if (desc is PrepareBatchesActivityDescription pb)
        {
            return (TResult)(object)(await PrepareBatches(pb));
        }
        else
        {
            throw new InvalidOperationException();
        }
    }

    private async Task<PrepareBatchesActivityDescriptionResult> PrepareBatches(PrepareBatchesActivityDescription pb)
    {
        return await session.TrackAggregateRoots<PrepareBatchesActivityDescriptionResult>(async _ =>
        {
            return await session.RetryOnExceptions<PrepareBatchesActivityDescriptionResult>(
                async _ =>
                    {
                        var shadowAndType = GetShadow(pb);
                        var result = (Task<PrepareBatchesActivityDescriptionResult>)
                            this.GetType().GetMethod("ContinueForShadow", BindingFlags.NonPublic | BindingFlags.Instance)!
                                .MakeGenericMethod(shadowAndType.ShadowType)
                                .Invoke(this, [shadowAndType.Shadow, pb])!;

                        return await result;
                    },
                    [typeof(VersionConcurrencyException), typeof(MongoDuplicateKeyException)], 
                MAX_RETRIES_FOR_SHADOW_UPDATE);
            
        });
    }

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "This is called through reflection.")]
	private async Task<PrepareBatchesActivityDescriptionResult> ContinueForShadow<TShadow>(TShadow shadow, PrepareBatchesActivityDescription pb) where TShadow : class, IShadow
    {
        var shadowRepository =
            (repositories.First(x => x is IShadowRepository<TShadow>) as IShadowRepository<TShadow>)!;

        TryAgain:
        var previous = await shadowRepository.FindOneByIdAsync(shadow.Id);
        if (previous == null || shadow.TimeStamp > previous.TimeStamp)
        {
            if (previous == null)
            {
                try
                {
                    await shadowRepository.InsertOneAsync(shadow);
				}
                catch (MongoWriteException me)
                {
                    if (me.WriteError.Category != ServerErrorCategory.DuplicateKey)
                        throw;
                    else
                    {
                        goto TryAgain;
                    }
                }
				catch (MongoDuplicateKeyException)
				{
					goto TryAgain;
				}
			}
            else
            {
                await shadowRepository.ReplaceOneAsync(shadow);
            }

            var managers = batchManagerFactory.GetManagersFromEvent(pb.Event, previous).ToList();
            var batches = new List<IBatch>();
            foreach (var batchManager in managers)
            {
                batches.AddRange(await batchManager.GetBatches(factory));
            }

            var dbBatches = await syncBatchRepository.FindManyByIdAsync(batches.Select(x => x.BatchId));

            return new PrepareBatchesActivityDescriptionResult(
                batches.Select(b => b.BatchId).ToList(),
                dbBatches.Select(x => (x.TrackerAssembly, x.TrackerType, x.TrackerRule)).Distinct().ToList());
        }
        else
        {
            return new PrepareBatchesActivityDescriptionResult([], []);
        }
    }

    private (IShadow Shadow, Type ShadowType) GetShadow(PrepareBatchesActivityDescription pb)
    {
        var shadowType = GetShadowTypeFromName(pb.Event.ShadowName);
        var shadow = pb.Event.ShadowJson.FromJsonString(shadowType) as IShadow ?? throw new InvalidOperationException("shadow of type IShadow not found");
        return (shadow, shadowType);
    }
}