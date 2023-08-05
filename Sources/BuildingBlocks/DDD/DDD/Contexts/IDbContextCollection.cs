namespace Pulsar.BuildingBlocks.DDD.Contexts;

public interface IDbContextCollection<TModel> where TModel : class, IAggregateRoot
{
    Task<TModel?> Get(ObjectId id);
    Task<List<TModel>> Get(params ObjectId[] ids);
    Task<List<TModel>> Get(IEnumerable<ObjectId> ids);
    Task Insert(TModel model);
    Task Insert(IEnumerable<TModel> models);
    Task<long> Replace(TModel model, long? version = null);
    Task<bool> Exists(ObjectId id);
    Task<bool> Exists(params ObjectId[] ids);
    Task<bool> Exists(IEnumerable<ObjectId> ids);
}
