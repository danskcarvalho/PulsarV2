using MongoDB.Bson;
using Pulsar.BuildingBlocks.DDD.Abstractions;

namespace DDD.Contracts;

public interface IDbContext
{
    Task<bool> Exists<TModel>(ObjectId id) where TModel : class, IAggregateRoot;
    Task<bool> Exists<TModel>(params ObjectId[] ids) where TModel : class, IAggregateRoot;
    Task<bool> Exists<TModel>(IEnumerable<ObjectId> ids) where TModel : class, IAggregateRoot;
    Task<TModel?> TryGet<TModel>(ObjectId id) where TModel : class, IAggregateRoot;
    Task<TModel> Get<TModel>(ObjectId id) where TModel : class, IAggregateRoot;
    Task<TModel> GetAndCache<TModel>(ObjectId id, string key) where TModel : class, IAggregateRoot;
    Task<TModel?> TryGetAndCache<TModel>(ObjectId id, string key) where TModel : class, IAggregateRoot;
    Task<List<TModel>> GetMany<TModel>(IEnumerable<ObjectId> ids) where TModel : class, IAggregateRoot;
}