using Pulsar.BuildingBlocks.DDD.Contexts;

namespace Pulsar.BuildingBlocks.DDD.Mongo.Contexts;

public class MongoDbContext : IDbContext
{
    private List<IIsRepository> _repositories;
    private Dictionary<Type, object> _repositoriesPerModel = new Dictionary<Type, object>();

    public MongoDbContext(IEnumerable<IIsRepository> repositories)
    {
        _repositories = repositories.ToList();
    }

    IDbContextCollection<TModel> IDbContext.GetCollection<TModel>()
    {
        if (_repositoriesPerModel.ContainsKey(typeof(TModel)))
            return (IDbContextCollection<TModel>)_repositoriesPerModel[typeof(TModel)];

        var repository = _repositories.First(r => r is IRepositoryBase<TModel>);
        var collection = new MongoDbContextCollection<TModel>((IRepositoryBase<TModel>)repository);
        _repositoriesPerModel[typeof(TModel)] = collection;
        return collection;
    }
}
