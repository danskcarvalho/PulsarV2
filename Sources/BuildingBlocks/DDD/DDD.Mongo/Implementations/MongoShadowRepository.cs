using Pulsar.BuildingBlocks.Sync.Contracts;

namespace Pulsar.BuildingBlocks.DDD.Mongo;

public class MongoShadowRepository<TModel> : MongoRepository<IShadowRepository<TModel>, TModel>, IShadowRepository<TModel>
    where TModel : class, IAggregateRoot
{
    private string _collectionName;
    public MongoShadowRepository(MongoDbSession? session, MongoDbSessionFactory sessionFactory) : base(session, sessionFactory)
    {
        _collectionName = GetCollectionName();
    }

    private string GetCollectionName()
    {
        var attr = typeof(TModel).GetCustomAttribute<ShadowAttribute>();
        if (attr == null)
        {
            throw new InvalidOperationException($"no ShadowAttribute on type {typeof(TModel).FullName}");
        }

        return $"_{ValidId(attr.Name)}_Shadow";
    }

    private string ValidId(string name)
    {
        return new string(name.Where(c => char.IsLetterOrDigit(c)).ToArray());
    }

    protected override string CollectionName => _collectionName;

    protected override IShadowRepository<TModel> Clone(MongoDbSession? session, MongoDbSessionFactory sessionFactory)
    {
        return new MongoShadowRepository<TModel>(session, sessionFactory);
    }
}
