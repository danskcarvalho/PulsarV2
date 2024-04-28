using System.Reflection;
using MediatR;
using MongoDB.Driver;
using Pulsar.BuildingBlocks.DDD;
using Pulsar.BuildingBlocks.DDD.Abstractions;
using Pulsar.BuildingBlocks.DDD.Contexts;
using Pulsar.BuildingBlocks.Sync.Contracts;
using Pulsar.BuildingBlocks.Sync.Functions.Abstractions;
using Pulsar.BuildingBlocks.Utils;

namespace Pulsar.BuildingBlocks.Sync.Functions.Implementations;

public class BatchActivity(
    IDbSession session,
    IBatchManagerFactory batchManagerFactory,
    IEnumerable<IIsRepository> repositories,
    ISyncDbContextFactory factory) : IBatchActivity
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
                foreach (var type in tys)
                {
                    _shadowTypes[type.Attribute.Name] = type.Type;
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
        else
        {
            throw new InvalidOperationException();
        }
    }

    private async Task ExecuteBatch(ExecuteBatchActivityDescription eb)
    {
        await session.TrackAggregateRoots(async ct =>
        {
            var batchManager = new BatchManagerForDatabase(factory);
            var batch = await batchManager.GetFromId(eb.BatchId);
            await batch.Execute();
            
            return Unit.Value;
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
                        var shadow = GetShadow(pb);
                        var result = (Task<PrepareBatchesActivityDescriptionResult>)
                            this.GetType().GetMethod("ContinueForShadow", BindingFlags.NonPublic)!
                                .MakeGenericMethod(shadow.ShadowType)
                                .Invoke(this, [shadow, pb])!;

                        return await result;
                    },
                    [typeof(VersionConcurrencyException), typeof(MongoDuplicateKeyException)], 
                MAX_RETRIES_FOR_SHADOW_UPDATE);
            
        });
    }

    private async Task<PrepareBatchesActivityDescriptionResult> ContinueForShadow<TShadow>(TShadow shadow, PrepareBatchesActivityDescription pb) where TShadow : class, IShadow
    {
        var shadowRepository = (repositories.First(x => x is IShadowRepository<TShadow>) as IShadowRepository<TShadow>)
                               ?? throw new InvalidOperationException(
                                   $"shadow repository for {typeof(TShadow).FullName} not found");

        var previous = await shadowRepository.FindOneByIdAsync(shadow.Id);
        if (previous == null || shadow.TimeStamp > previous.TimeStamp)
        {
            var previousVersion = previous?.Version;
            shadow.Version = Math.Max(shadow.Version, previousVersion ?? 0) + 1;

            if (previous == null)
            {
                await shadowRepository.InsertOneAsync(shadow);
            }
            else
            {
                await shadowRepository.ReplaceOneAsync(shadow, previousVersion).CheckModified();
            }

            var managers = batchManagerFactory.GetManagersFromEvent(pb.Event, previous).ToList();
            var batches = new List<IBatch>();
            foreach (var batchManager in managers)
            {
                batches.AddRange(await batchManager.GetBatches(factory));
            }

            return new PrepareBatchesActivityDescriptionResult(batches.Select(b => b.BatchId).ToList());
        }
        else
        {
            return new PrepareBatchesActivityDescriptionResult([]);
        }
    }

    private (IShadow Shadow, Type ShadowType) GetShadow(PrepareBatchesActivityDescription pb)
    {
        var shadowType = GetShadowTypeFromName(pb.Event.ShadowName);
        var shadow = pb.Event.ShadowJson.FromJsonString(shadowType) as IShadow ?? throw new InvalidOperationException("shadow of type IShadow not found");
        return (shadow, shadowType);
    }
}