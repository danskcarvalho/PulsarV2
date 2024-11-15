﻿namespace Pulsar.BuildingBlocks.DDD.Contexts;

public class DbContextCollection<TModel> where TModel : class, IAggregateRoot
{
    private readonly IRepositoryBase<TModel> _repository;
    private Dictionary<string, TModel?> _cachedObjects = new Dictionary<string, TModel?>();

    public DbContextCollection(IRepositoryBase<TModel> repository)
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

    public async Task<TModel?> GetAndCache(ObjectId id, string key)
    {
        if (this._cachedObjects.ContainsKey(key) && this._cachedObjects[key] != null && this._cachedObjects[key]?.Id == id)
            return _cachedObjects[key];
        if (this._cachedObjects.ContainsKey(key) && this._cachedObjects[key] == null)
            return null;

        _cachedObjects[key] = await Get(id);
        return _cachedObjects[key];
    }

    public Task Insert(TModel model)
    {
        return _repository.InsertOneAsync(model);
    }

    public Task Insert(IEnumerable<TModel> models)
    {
        return _repository.InsertManyAsync(models);
    }

    public Task<long> Replace(TModel model)
    {
        return _repository.ReplaceOneAsync(model);
    }
}
