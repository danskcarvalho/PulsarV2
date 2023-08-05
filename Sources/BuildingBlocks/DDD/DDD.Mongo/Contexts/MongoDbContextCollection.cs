using Pulsar.BuildingBlocks.DDD.Contexts;

namespace Pulsar.BuildingBlocks.DDD.Mongo.Contexts;

public class MongoDbContextCollection<TModel> : IDbContextCollection<TModel> where TModel : class, IAggregateRoot
{
    private IRepositoryBase<TModel> _repository;

    public MongoDbContextCollection(IRepositoryBase<TModel> repository)
    {
        this._repository = repository;
    }

    public Task<bool> Exists(ObjectId id)
    {
        return _repository.OneExistsAsync(id);
    }

    public Task<bool> Exists(params ObjectId[] ids)
    {
        return _repository.AllExistsAsync(ids);
    }

    public Task<bool> Exists(IEnumerable<ObjectId> ids)
    {
        return _repository.AllExistsAsync(ids);
    }

    public Task<TModel?> Get(ObjectId id)
    {
        return _repository.FindOneByIdAsync(id);
    }

    public Task<List<TModel>> Get(params ObjectId[] ids)
    {
        return _repository.FindManyByIdAsync(ids);
    }

    public Task<List<TModel>> Get(IEnumerable<ObjectId> ids)
    {
        return _repository.FindManyByIdAsync(ids);
    }

    public Task Insert(TModel model)
    {
        return _repository.InsertOneAsync(model);
    }

    public Task Insert(IEnumerable<TModel> models)
    {
        return _repository.InsertManyAsync(models);
    }

    public Task<long> Replace(TModel model, long? version = null)
    {
        return _repository.ReplaceOneAsync(model, version);
    }
}
