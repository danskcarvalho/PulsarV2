using Pulsar.BuildingBlocks.DDD.Mongo.Implementations;

namespace Pulsar.BuildingBlocks.Sync.Functions.Entities;

public class SyncBatchMongoRepository : MongoRepository<ISyncBatchRepository, SyncBatch>, ISyncBatchRepository
{
    public SyncBatchMongoRepository(MongoDbSession? session, MongoDbSessionFactory sessionFactory) : base(session, sessionFactory)
    {
    }

    protected override string CollectionName => "__SyncBatches";
    protected override ISyncBatchRepository Clone(MongoDbSession? session, MongoDbSessionFactory sessionFactory)
    {
        return new SyncBatchMongoRepository(session, sessionFactory);
    }
}