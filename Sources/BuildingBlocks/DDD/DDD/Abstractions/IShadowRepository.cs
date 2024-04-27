namespace Pulsar.BuildingBlocks.DDD;

public interface IShadowRepository<TModel> : IRepository<IShadowRepository<TModel>, TModel>, IShadowRepositoryBase<TModel>
    where TModel : class, IAggregateRoot
{
}

public interface IShadowRepositoryBase<TModel> : IRepositoryBase<TModel> where TModel : class, IAggregateRoot
{
}