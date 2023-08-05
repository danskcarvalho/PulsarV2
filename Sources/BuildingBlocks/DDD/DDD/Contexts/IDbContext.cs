namespace Pulsar.BuildingBlocks.DDD.Contexts;

public interface IDbContext
{
    IDbContextCollection<TModel> GetCollection<TModel>() where TModel : class, IAggregateRoot;
}
