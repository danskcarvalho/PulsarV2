using MongoDB.Bson;
using Pulsar.BuildingBlocks.Sync.Functions.Abstractions;
using Pulsar.BuildingBlocks.Sync.Functions.Entities;

namespace Pulsar.BuildingBlocks.Sync.Functions.Implementations;

public class BatchManagerForDatabase(ISyncDbContextFactory factory) : IBatchManagerForDatabase
{
    public async Task<IBatch> GetFromId(ObjectId batchId)
    {
        return await factory.Execute<IBatch>(async ctx =>
        {
            var syncBatch = await ctx.SyncBatchRepository.FindOneByIdAsync(batchId);
            if (syncBatch == null)
            {
                throw new InvalidOperationException("batch not found");
            }
            return CreateBatch(syncBatch);
        });
    }

    private IBatch CreateBatch(SyncBatch syncBatch)
    {
        var entityAssembly = AppDomain.CurrentDomain.GetAssemblies().First(x => x.FullName == syncBatch.EntityAssembly);
        var entityType = entityAssembly.GetType(syncBatch.EntityType);

        var shadowAssembly = AppDomain.CurrentDomain.GetAssemblies().First(x => x.FullName == syncBatch.ShadowAssembly);
        var shadowType = shadowAssembly.GetType(syncBatch.ShadowType);

        if (entityType == null)
        {
            throw new InvalidOperationException($"entity type not found {syncBatch.EntityType}");
        }
        
        if (shadowType == null)
        {
            throw new InvalidOperationException($"shado type not found {syncBatch.ShadowType}");
        }

        return NewBatch(shadowType, entityType, syncBatch);
    }

    private IBatch NewBatch(Type shadowType, Type entityType, SyncBatch syncBatch)
    {
        return (IBatch)Activator.CreateInstance(typeof(Batch<,>).MakeGenericType(shadowType, entityType), 
            factory,
            syncBatch.Id)!;
    }
}