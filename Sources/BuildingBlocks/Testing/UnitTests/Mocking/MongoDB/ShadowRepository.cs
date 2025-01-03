﻿using Pulsar.BuildingBlocks.DDD;
using Pulsar.BuildingBlocks.Sync.Contracts;

namespace Pulsar.BuildingBlocks.UnitTests.Mocking.MongoDB;

public class ShadowRepository<TModel> : IShadowRepository<TModel>
    where TModel : class, IAggregateRoot
{
    protected IMockedDbSession? Session { get; private set; }
    protected IMockedDbSessionFactory SessionFactory { get; private set; }
    protected IMockedCollection<TModel> Collection { get; private set; }
    protected bool IsSessionless => Session is null;

    public ShadowRepository(IMockedDbSession? session, IMockedDbSessionFactory sessionFactory)
    {
        Session = session;
        SessionFactory = sessionFactory;
        Collection = sessionFactory.Database.GetCollection<TModel>(CollectionName);
    }
    protected IMockedCollection<T> GetCollection<T>(string collectionName) where T : class
    {
        return SessionFactory.Database.GetCollection<T>(collectionName);
    }

    protected virtual string CollectionName => typeof(IShadow).IsAssignableFrom(typeof(TModel))
        ? Shadow<TModel>.GetCollectionName()
        : throw new InvalidOperationException("implement CollectionName");
    
    private async Task DispatchDomainEvents(TModel model)
    {
        if (Session != null)
        {
            await Session.DispatchDomainEvents(model);
        }
    }

    private IShadowRepository<TModel> Clone(IMockedDbSession? session, IMockedDbSessionFactory sessionFactory)
    {
        return new ShadowRepository<TModel>(session, sessionFactory);
    }

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
    public async Task<long> DeleteOneAsync(TModel model, bool checkModified = true, CancellationToken ct = default)
    {
        var r = await DeleteOneByIdAsync(model.Id, model.LastVersion ?? model.Version, ct);
        if (checkModified)
        {
            r.CheckModified();
        }
        if (r != 0)
        {
            await DispatchDomainEvents(model);
        }

        return r;
    }

    public IShadowRepository<TModel> EscapeSession()
    {
        var cloned = Clone(Session, SessionFactory);
        return cloned;
    }

    IShadowRepository<TModel> IRepository<IShadowRepository<TModel>, TModel>.WithIsolation(IsolationLevel level)
    {
        return new ShadowRepository<TModel>(Session, SessionFactory);
    }

    public async Task<List<TModel>> FindManyAsync(IFindSpecification<TModel> spec, CancellationToken ct = default)
    {
        var r = await ToListAsync(Collection.FindAsync(spec));
        return r;
    }

    public async Task<List<TProjection>> FindManyAsync<TProjection>(IFindSpecification<TModel, TProjection> spec, CancellationToken ct = default)
    {
        return await ToListAsync(Collection.FindAsync<TProjection>(spec));
    }

    public async Task<List<TModel>> FindManyByIdAsync(IEnumerable<ObjectId> ids, CancellationToken ct = default)
    {
        var idList = new HashSet<ObjectId>(ids);

        var r = await ToListAsync(Collection.FindAsync(new FindSpecificationWrapper<TModel>(Find.Where<TModel>(x => idList.Contains(x.Id)).Build())));
        return r;
    }

    public async Task<TModel?> FindOneAsync(IFindSpecification<TModel> spec, CancellationToken ct = default)
    {
        var r = await ToListAsync(Collection.FindAsync(spec));
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
        return r.FirstOrDefault();
    }

    public async Task InsertManyAsync(IEnumerable<TModel> items, CancellationToken ct = default)
    {
        await Collection.InsertManyAsync(items);

        foreach (var root in items)
        {
            await DispatchDomainEvents(root);
        }
    }

    public async Task InsertOneAsync(TModel item, CancellationToken ct = default)
    {
        await Collection.InsertManyAsync(new TModel[] { item });

        await DispatchDomainEvents(item);
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

    public async Task<long> ReplaceOneAsync(TModel model, bool checkModified = true, CancellationToken ct = default)
    {
        var r = await Collection.ReplaceAsync(model, model.Id, model.LastVersion ?? model.Version, "Version");
        if (checkModified)
        {
            r.CheckModified();
        }
        if (r != 0)
        {
            await DispatchDomainEvents(model);
        }
        return r;
    }

    public async Task<long> UpdateManyAsync(IUpdateSpecification<TModel> spec, CancellationToken ct = default)
    {
        return await Collection.UpdateManyAsync(new UpdateVersionAndAlso<TModel>(spec));
    }

    public async Task<long> UpdateOneAsync(IUpdateSpecification<TModel> spec, CancellationToken ct = default)
    {
        return await Collection.UpdateManyAsync(new UpdateVersionAndAlso<TModel>(spec), 1);
    }

    IShadowRepository<TModel> IRepository<IShadowRepository<TModel>, TModel>.EscapeSession()
    {
        return new ShadowRepository<TModel>(this.Session, this.SessionFactory);
    }

    public IShadowRepository<TModel> WithIsolation(IsolationLevel level)
    {
        return Clone(Session, SessionFactory);
    }
}
