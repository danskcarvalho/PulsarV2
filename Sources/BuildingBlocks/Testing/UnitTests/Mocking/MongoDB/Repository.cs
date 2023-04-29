namespace Pulsar.BuildingBlocks.UnitTests.Mocking.MongoDB;

public abstract class Repository<TSelf, TModel> : IRepository<TSelf, TModel>
    where TModel : class, IAggregateRoot
    where TSelf : IRepositoryBase<TModel>
{
    protected IMockedDbSession? Session { get; private set; }
    protected IMockedDbSessionFactory SessionFactory { get; private set; }
    protected IMockedCollection<TModel> Collection { get; private set; }
    protected bool IsSessionless => Session is null;

    public Repository(IMockedDbSession? session, IMockedDbSessionFactory sessionFactory)
    {
        Session = session;
        SessionFactory = sessionFactory;
        Collection = sessionFactory.Database.GetCollection<TModel>(CollectionName);
    }
    protected IMockedCollection<T> GetCollection<T>(string collectionName) where T : class
    {
        return SessionFactory.Database.GetCollection<T>(collectionName);
    }

    protected abstract string CollectionName { get; }
    public void Track(TModel? model)
    {
        if (model == null)
            return;
        if (Session is not null)
            Session.TrackAggregateRoot(model);
    }
    protected abstract TSelf Clone(IMockedDbSession? session, IMockedDbSessionFactory sessionFactory);

    public async Task<bool> AllExistsAsync(IEnumerable<ObjectId> ids, IFindSpecification<TModel>? predicate = null, CancellationToken ct = default)
    {
        var idList = ids.ToList();
        Expression<Func<TModel, bool>> predicateExpression = x => idList.Contains(x.Id);
        if (predicate != null)
        {
            var spec = predicate.GetSpec();
            predicateExpression = predicateExpression.AndAlso(spec.Predicate);
        }

        var r = await ToListAsync(Collection.FindAsync(new FindSpecificationWrapper<TModel>(Find.Where(predicateExpression).Build())));

        HashSet<ObjectId> found = new HashSet<ObjectId>(r.Select(x => x.Id));
        return idList.All(x => found.Contains(x));
    }

    private async Task<List<TOther>> ToListAsync<TOther>(IAsyncEnumerable<TOther> asyncEnumerable)
    {
        var list = new List<TOther>();

        await foreach (var item in asyncEnumerable)
        {
            list.Add(item);
        }

        return list;
    }

    public async Task<long> DeleteManyAsync(IDeleteSpecification<TModel> spec, CancellationToken ct = default)
    {
        return await Collection.DeleteManyAsync(spec);
    }

    public async Task<long> DeleteOneAsync(IDeleteSpecification<TModel> spec, CancellationToken ct = default)
    {
        return await Collection.DeleteManyAsync(spec, 1);
    }

    public async Task<long> DeleteManyByIdAsync(IEnumerable<ObjectId> ids, CancellationToken ct = default)
    {
        var idSet = new HashSet<ObjectId>(ids);
        return await Collection.DeleteManyAsync(new DeleteSpecificationWrapper<TModel>(Delete.Where<TModel>(x => idSet.Contains(x.Id)).Build()));
    }

    public async Task<long> DeleteOneByIdAsync(ObjectId id, long? version = null, CancellationToken ct = default)
    {
        if (version != null)
            return await Collection.DeleteManyAsync(new DeleteSpecificationWrapper<TModel>(Delete.Where<TModel>(x => x.Id == id && x.Version == version).Build()), 1);
        else

            return await Collection.DeleteManyAsync(new DeleteSpecificationWrapper<TModel>(Delete.Where<TModel>(x => x.Id == id).Build()), 1);
    }
    public async Task<long> DeleteOneAsync(TModel model, long? version = null, CancellationToken ct = default)
    {
        var r = await DeleteOneByIdAsync(model.Id, version, ct);
        Track(model);
        return r;
    }

    public TSelf EscapeSession()
    {
        var cloned = Clone(Session, SessionFactory);
        return cloned;
    }

    public async Task<List<TModel>> FindManyAsync(IFindSpecification<TModel> spec, bool noTracking = false, CancellationToken ct = default)
    {
        var r = await ToListAsync(Collection.FindAsync(spec));

        if (!noTracking)
        {
            foreach (var root in r)
                Track(root);
        }

        return r;
    }

    public async Task<List<TProjection>> FindManyAsync<TProjection>(IFindSpecification<TModel, TProjection> spec, CancellationToken ct = default)
    {
        return await ToListAsync(Collection.FindAsync<TProjection>(spec));
    }

    public async Task<List<TModel>> FindManyByIdAsync(IEnumerable<ObjectId> ids, bool noTracking = false, CancellationToken ct = default)
    {
        var idList = new HashSet<ObjectId>(ids);

        var r = await ToListAsync(Collection.FindAsync(new FindSpecificationWrapper<TModel>(Find.Where<TModel>(x => idList.Contains(x.Id)).Build())));

        if (!noTracking)
        {
            foreach (var root in r)
                Track(root);
        }

        return r;
    }

    public async Task<TModel?> FindOneAsync(IFindSpecification<TModel> spec, CancellationToken ct = default)
    {
        var r = await ToListAsync(Collection.FindAsync(spec));

        Track(r.FirstOrDefault());
        return r.FirstOrDefault();
    }

    public async Task<TProjection?> FindOneAsync<TProjection>(IFindSpecification<TModel, TProjection> spec, CancellationToken ct = default)
    {
        var r =  await ToListAsync(Collection.FindAsync(spec));
        return r.FirstOrDefault();
    }

    public async Task<TModel?> FindOneByIdAsync(ObjectId id, CancellationToken ct = default)
    {
        var r = await ToListAsync(Collection.FindAsync(new FindSpecificationWrapper<TModel>(Find.Where<TModel>(x => x.Id == id).Build())));

        Track(r.FirstOrDefault());
        return r.FirstOrDefault();
    }

    public async Task InsertManyAsync(IEnumerable<TModel> items, CancellationToken ct = default)
    {
        await Collection.InsertManyAsync(items);

        foreach (var root in items)
            Track(root);
    }

    public async Task InsertOneAsync(TModel item, CancellationToken ct = default)
    {
        await Collection.InsertManyAsync(new TModel[] { item });

        Track(item);
    }

    public async Task<bool> OneExistsAsync(ObjectId id, IFindSpecification<TModel>? predicate = null, CancellationToken ct = default)
    {
        Expression<Func<TModel, bool>> predicateExpression = x => x.Id == id;
        if (predicate != null)
        {
            var spec = predicate.GetSpec();
            predicateExpression = predicateExpression.AndAlso(spec.Predicate);
        }
        var r = await ToListAsync(Collection.FindAsync(new FindSpecificationWrapper<TModel>(Find.Where(predicateExpression).Build())));

        HashSet<ObjectId> found = new HashSet<ObjectId>(r.Select(x => x.Id));
        return found.Contains(id);
    }

    public async Task<long> ReplaceOneAsync(TModel model, long? version = null, CancellationToken ct = default)
    {
        return await Collection.ReplaceAsync(model, model.Id, version, "Version");
    }

    public async Task<long> UpdateManyAsync(IUpdateSpecification<TModel> spec, CancellationToken ct = default)
    {
        return await Collection.UpdateManyAsync(spec);
    }

    public async Task<long> UpdateOneAsync(IUpdateSpecification<TModel> spec, CancellationToken ct = default)
    {
        return await Collection.UpdateManyAsync(spec, 1);
    }

    public TSelf WithIsolation(IsolationLevel level)
    {
        return Clone(Session, SessionFactory);
    }
}
