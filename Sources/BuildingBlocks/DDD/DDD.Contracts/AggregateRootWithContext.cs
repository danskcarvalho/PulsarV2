using MongoDB.Bson;
using Pulsar.BuildingBlocks.DDD;
using Pulsar.BuildingBlocks.DDD.Abstractions;

namespace DDD.Contracts
{
    public abstract class AggregateRootWithContext<TSelf> : AggregateRoot where TSelf : class, IAggregateRoot
    {
        public static Task<bool> Exists(ObjectId id) => DbContext.Current.Exists<TSelf>(id);
        public static Task<bool> Exists(params ObjectId[] ids) => DbContext.Current.Exists<TSelf>(ids);
        public static Task<bool> Exists(IEnumerable<ObjectId> ids) => DbContext.Current.Exists<TSelf>(ids);
        public static Task<TSelf?> TryGet(ObjectId id) => DbContext.Current.TryGet<TSelf>(id);
        public static async Task<TSelf> Get(ObjectId id) => await DbContext.Current.Get<TSelf>(id);
        public static async Task<TSelf> GetAndCache(ObjectId id, string key) => await DbContext.Current.GetAndCache<TSelf>(id, key);
        public static Task<TSelf?> TryGetAndCache(ObjectId id, string key) => DbContext.Current.TryGetAndCache<TSelf>(id, key);
        public static Task<List<TSelf>> GetMany(IEnumerable<ObjectId> ids) => DbContext.Current.GetMany<TSelf>(ids);

        public AggregateRootWithContext() { }
        public AggregateRootWithContext(ObjectId id) : base(id)
        {
        }

    }
}
