namespace Pulsar.BuildingBlocks.DDD.Abstractions;

public interface IRepository<TSelf, TModel> : IRepositoryBase<TModel> 
    where TModel : class, IAggregateRoot 
    where TSelf : IRepositoryBase<TModel>
{
    TSelf EscapeSession();
    TSelf WithIsolation(IsolationLevel level);
}

public interface IRepositoryBase<TModel> : IIsRepository where TModel : class, IAggregateRoot
{
    Task InsertOneAsync(TModel item, CancellationToken ct = default);
    Task InsertManyAsync(IEnumerable<TModel> items, CancellationToken ct = default);
    Task<long> DeleteOneByIdAsync(ObjectId id, long? version = null, CancellationToken ct = default);
    Task<long> DeleteOneAsync(TModel model, bool checkModified = true, CancellationToken ct = default);
    Task<long> DeleteManyByIdAsync(IEnumerable<ObjectId> ids, CancellationToken ct = default);
    Task<long> DeleteManyAsync(IDeleteSpecification<TModel> spec, CancellationToken ct = default);
    Task<long> DeleteOneAsync(IDeleteSpecification<TModel> spec, CancellationToken ct = default);
    Task<long> ReplaceOneAsync(TModel model, bool checkModified = true, CancellationToken ct = default);
    Task<long> UpdateOneAsync(IUpdateSpecification<TModel> spec, CancellationToken ct = default);
    Task<long> UpdateManyAsync(IUpdateSpecification<TModel> spec, CancellationToken ct = default);
    Task<bool> OneExistsAsync(ObjectId id, IFindSpecification<TModel>? predicate = null, CancellationToken ct = default);
    Task<bool> AllExistsAsync(IEnumerable<ObjectId> ids, IFindSpecification<TModel>? predicate = null, CancellationToken ct = default);
    Task<TModel?> FindOneByIdAsync(ObjectId id, CancellationToken ct = default);
    Task<List<TModel>> FindManyByIdAsync(IEnumerable<ObjectId> ids, CancellationToken ct = default);
    Task<TModel?> FindOneAsync(IFindSpecification<TModel> spec, CancellationToken ct = default);
    Task<TProjection?> FindOneAsync<TProjection>(IFindSpecification<TModel, TProjection> spec, CancellationToken ct = default);
    Task<List<TModel>> FindManyAsync(IFindSpecification<TModel> spec, CancellationToken ct = default);
    Task<List<TProjection>> FindManyAsync<TProjection>(IFindSpecification<TModel, TProjection> spec, CancellationToken ct = default);
}

public interface IIsRepository
{

}
