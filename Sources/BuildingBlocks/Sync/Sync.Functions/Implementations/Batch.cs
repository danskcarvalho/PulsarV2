using System.Reflection;
using MongoDB.Bson;
using Pulsar.BuildingBlocks.DDD.Abstractions;
using Pulsar.BuildingBlocks.Sync.Contracts;
using Pulsar.BuildingBlocks.Sync.Domain;
using Pulsar.BuildingBlocks.Sync.Functions.Abstractions;
using Pulsar.BuildingBlocks.Sync.Functions.Entities;
using Pulsar.BuildingBlocks.Utils;

namespace Pulsar.BuildingBlocks.Sync.Functions.Implementations;

public class Batch<TShadow, TEntity>(ISyncDbContextFactory factory, ObjectId batchId)
    : IBatchForShadowAndEntity<TShadow, TEntity>
    where TEntity : class, IAggregateRoot
    where TShadow : class, IShadow
{
    private readonly ISyncDbContextFactory _factory = factory;

    public async Task Execute()
    {
        await _factory.Execute<TEntity, int>(async ctx =>
        {
            var batch = await ctx.SyncBatchRepository.FindOneByIdAsync(BatchId);
            batch = batch ?? throw new InvalidOperationException($"batch not found {BatchId}");

            var trackerAssembly = AppDomain.CurrentDomain.GetAssemblies().First(x => x.FullName == batch.TrackerAssembly);
            var trackerType = trackerAssembly.GetType(batch.TrackerType) ??
                              throw new InvalidOperationException($"type not found {batch.TrackerType}");

            var field = trackerType.GetField(batch.TrackerRule, BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic)
                ?? throw new InvalidOperationException($"no field named {batch.TrackerRule} on type {batch.TrackerType}");
            var rule = (TrackerUpdateAction<TEntity>)field.GetValue(null)!;

            await ExecuteBatch(ctx.EntityRepository, rule, batch);
            
            return 0;
        });
    }

    private async Task ExecuteBatch(IRepositoryBase<TEntity> entityRepository, TrackerUpdateAction<TEntity> rule, SyncBatch batch)
    {
        if (rule.UpdateFunction == null)
        {
            return;
        }
        var shadowAssembly = AppDomain.CurrentDomain.GetAssemblies().First(x => x.FullName == batch.ShadowAssembly);
        var shadowType = shadowAssembly.GetType(batch.ShadowType) ??
                         throw new InvalidOperationException($"type not found {batch.ShadowType}");

        var shadow = batch.ShadowJson.FromJsonString(shadowType)!;
        var updSpec = rule.UpdateFunction(shadow);
        
        await entityRepository.UpdateManyAsync(new UpdateManyById(updSpec, batch.EntitiesToUpdate));
    }

    public ObjectId BatchId { get; } = batchId;

    class UpdateManyById(IUpdateSpecification<TEntity> updateSpecification, List<ObjectId> ids) : IUpdateSpecification<TEntity>
    {
        public UpdateSpecification<TEntity> GetSpec()
        {
            return Update.Where<TEntity>(x => ids.Contains(x.Id)).CopyCommandsFrom(updateSpecification).Build();
        }
    }
}