using Pulsar.BuildingBlocks.DDD.Contexts;

namespace Pulsar.BuildingBlocks.DDD
{
    public abstract class AggregateRootWithContext<TSelf> : AggregateRoot where TSelf : class, IAggregateRoot
    {
        public static Task<bool> Exists(ObjectId id) => DbContext.Current.GetCollection<TSelf>().Exists(id);
        public static Task<bool> Exists(params ObjectId[] ids) => DbContext.Current.GetCollection<TSelf>().Exists(ids);
        public static Task<bool> Exists(IEnumerable<ObjectId> ids) => DbContext.Current.GetCollection<TSelf>().Exists(ids);
        public static Task<TSelf?> TryGet(ObjectId id) => DbContext.Current.GetCollection<TSelf>().Get(id);
        public static async Task<TSelf> Get(ObjectId id) => (await DbContext.Current.GetCollection<TSelf>().Get(id)) ??
            throw new InvalidOperationException($"no entity of type {typeof(TSelf).Name} with _id {id}");
        public static async Task<TSelf> GetAndCache(ObjectId id, string key) => (await DbContext.Current.GetCollection<TSelf>().GetAndCache(id, key)) ??
            throw new InvalidOperationException($"no entity of type {typeof(TSelf).Name} with _id {id}");
        public static Task<TSelf?> TryGetAndCache(ObjectId id, string key) => DbContext.Current.GetCollection<TSelf>().GetAndCache(id, key);
        public static Task<List<TSelf>> GetMany(IEnumerable<ObjectId> ids) => DbContext.Current.GetCollection<TSelf>().Get(ids);

        public AggregateRootWithContext() { }
        public AggregateRootWithContext(ObjectId id) : base(id)
        {
        }

    }
}
