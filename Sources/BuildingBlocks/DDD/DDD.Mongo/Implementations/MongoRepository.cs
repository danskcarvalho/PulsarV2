using MongoDB.Driver;
using Pulsar.BuildingBlocks.Utils;
using System.Linq.Expressions;

namespace Pulsar.BuildingBlocks.DDD.Mongo.Implementations;

public abstract class MongoRepository<TSelf, TModel> : IRepository<TSelf, TModel>
    where TModel : class, IAggregateRoot
    where TSelf : IRepositoryBase<TModel>
{
    protected MongoDbSession? Session { get; private set; }
    protected MongoDbSessionFactory SessionFactory { get; private set; }
    protected IMongoCollection<TModel> Collection { get; private set; }
    protected bool IsSessionless => Session is null;
    protected IsolationLevel? IsolationLevel { get; private set; }
    protected abstract string CollectionName { get; }

    public MongoRepository(MongoDbSession? session, MongoDbSessionFactory sessionFactory)
    {
        Session = session;
        SessionFactory = sessionFactory;
        Collection = sessionFactory.Database.GetCollection<TModel>(CollectionName);
        if (!ApplyIsolationLevelFromSession())
            Collection = Collection.WithWriteConcern(WriteConcern.WMajority).WithReadConcern(ReadConcern.Majority).WithReadPreference(ReadPreference.Primary);
    }

    protected IMongoCollection<T> GetCollection<T>(string collectionName)
    {
        var collection = SessionFactory.Database.GetCollection<T>(collectionName);
        if (!ApplyIsolationLevelFromSession(ref collection))
            collection = collection.WithWriteConcern(WriteConcern.WMajority).WithReadConcern(ReadConcern.Majority).WithReadPreference(ReadPreference.Primary);
        return collection;
    }

    private IsolationLevel? lastSeenIsolationLevelFromSession = null;
    private bool ApplyIsolationLevelFromSession()
    {
        if (IsolationLevel == null)
        {
            if (Session is not null && Session.DefaultIsolationLevel != null)
            {
                if (lastSeenIsolationLevelFromSession != null && lastSeenIsolationLevelFromSession != Session.DefaultIsolationLevel && !Session.IsInTransaction)
                {
                    ApplyIsolationLevel(Session.DefaultIsolationLevel.Value);
                    return true;
                }
            }
        }
        return false;
    }

    private bool ApplyIsolationLevelFromSession<T>(ref IMongoCollection<T> collection)
    {
        if (IsolationLevel == null)
        {
            if (Session is not null && Session.DefaultIsolationLevel != null)
            {
                if (lastSeenIsolationLevelFromSession != null && lastSeenIsolationLevelFromSession != Session.DefaultIsolationLevel && !Session.IsInTransaction)
                {
                    collection = ApplyIsolationLevel(collection, Session.DefaultIsolationLevel.Value);
                    return true;
                }
            }
        }
        return false;
    }
    protected abstract TSelf Clone(MongoDbSession? session, MongoDbSessionFactory sessionFactory);

    public async Task<bool> AllExistsAsync(IEnumerable<ObjectId> ids, IFindSpecification<TModel>? predicate = null, CancellationToken ct = default)
    {
        ApplyIsolationLevelFromSession();
        var idList = ids.ToList();
        Expression<Func<TModel, bool>> predicateExpression = x => idList.Contains(x.Id);
        if (predicate != null)
        {
            var spec = predicate.GetSpec();
            predicateExpression = predicateExpression.AndAlso(spec.Predicate);
        }
        var projectionDef = Builders<TModel>.Projection.Expression(x => new ExistIdModel { Id = x.Id });
        List<ExistIdModel> r;

        if (Session is null || Session.CurrentHandle is null)
            r = await (await Collection.FindAsync(predicateExpression, new FindOptions<TModel, ExistIdModel> { Projection = projectionDef },
                cancellationToken: ct)).ToListAsync();
        else
            r = await (await Collection.FindAsync(Session.CurrentHandle, predicateExpression, new FindOptions<TModel, ExistIdModel> { Projection = projectionDef },
                cancellationToken: ct)).ToListAsync();

        HashSet<ObjectId> found = new HashSet<ObjectId>(r.Select(x => x.Id));
        return idList.All(x => found.Contains(x));
    }

    public async Task<long> DeleteManyAsync(IDeleteSpecification<TModel> spec, CancellationToken ct = default)
    {
        ApplyIsolationLevelFromSession();
        var finalSpec = spec.GetSpec();
        var where = finalSpec.Predicate;

        DeleteResult r;
        if (Session is null || Session.CurrentHandle is null)
            r = await Collection.DeleteManyAsync(where, cancellationToken: ct);
        else
            r = await Collection.DeleteManyAsync(Session.CurrentHandle, where, cancellationToken: ct);

        return r.IsAcknowledged ? r.DeletedCount : 0;
    }

    public async Task<long> DeleteOneAsync(IDeleteSpecification<TModel> spec, CancellationToken ct = default)
    {
        ApplyIsolationLevelFromSession();
        var finalSpec = spec.GetSpec();
        var where = finalSpec.Predicate;

        DeleteResult r;
        if (Session is null || Session.CurrentHandle is null)
            r = await Collection.DeleteOneAsync(where, cancellationToken: ct);
        else
            r = await Collection.DeleteOneAsync(Session.CurrentHandle, where, cancellationToken: ct);

        return r.IsAcknowledged ? r.DeletedCount : 0;
    }

    public async Task<long> DeleteManyByIdAsync(IEnumerable<ObjectId> ids, CancellationToken ct = default)
    {
        ApplyIsolationLevelFromSession();
        var listIds = ids.ToList();
        var filter = Builders<TModel>.Filter.In("_id", listIds);

        DeleteResult r;
        if (Session is null || Session.CurrentHandle is null)
            r = await Collection.DeleteManyAsync(filter, cancellationToken: ct);
        else
            r = await Collection.DeleteManyAsync(Session.CurrentHandle, filter, cancellationToken: ct);

        return r.IsAcknowledged ? r.DeletedCount : 0;
    }

    public async Task<long> DeleteOneByIdAsync(ObjectId id, long? version = null, CancellationToken ct = default)
    {
        ApplyIsolationLevelFromSession();
        var filter = Builders<TModel>.Filter.Eq("_id", id);
        if (version != null)
            filter = Builders<TModel>.Filter.And(filter, Builders<TModel>.Filter.Eq("Version", version));

        DeleteResult r;

        if (Session is null || Session.CurrentHandle is null)
            r = await Collection.DeleteOneAsync(filter, cancellationToken: ct);
        else
            r = await Collection.DeleteOneAsync(Session.CurrentHandle, filter, cancellationToken: ct);

        return r.IsAcknowledged ? r.DeletedCount : 0;
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

    private async Task DispatchDomainEvents(TModel model)
    {
        if (Session != null)
        {
            await Session.DispatchDomainEvents(model);
        }
    }

    public TSelf EscapeSession()
    {
        var cloned = Clone(null, SessionFactory);
        return cloned;
    }

    public async Task<List<TModel>> FindManyAsync(IFindSpecification<TModel> spec, CancellationToken ct = default)
    {
        ApplyIsolationLevelFromSession();
        var finalSpec = spec.GetSpec();
        var where = finalSpec.Predicate;
        SortDefinition<TModel>? sort = null;
        foreach (var col in finalSpec.OrderBy)
        {
            if (col is Ascending<TModel> a)
            {
                if (sort == null)
                    sort = Builders<TModel>.Sort.Ascending(a.Expression);
                else
                    sort = sort.Ascending(a.Expression);
            }
            else if (col is Descending<TModel> b)
            {
                if (sort == null)
                    sort = Builders<TModel>.Sort.Descending(b.Expression);
                else
                    sort = sort.Descending(b.Expression);
            }
        }

        List<TModel> r;
        if (Session is null || Session.CurrentHandle is null)
            r = await (await Collection.FindAsync(where, new FindOptions<TModel, TModel>
            {
                Sort = sort,
                Limit = finalSpec.Limit,
                Skip = finalSpec.Skip
            }, cancellationToken: ct)).ToListAsync();
        else
            r = await (await Collection.FindAsync(Session.CurrentHandle, where, new FindOptions<TModel, TModel>
            {
                Sort = sort,
                Limit = finalSpec.Limit,
                Skip = finalSpec.Skip
            }, cancellationToken: ct)).ToListAsync();

        return r;
    }

    public async Task<List<TProjection>> FindManyAsync<TProjection>(IFindSpecification<TModel, TProjection> spec, CancellationToken ct = default)
    {
        ApplyIsolationLevelFromSession();
        var finalSpec = spec.GetSpec();
        var where = finalSpec.Predicate;
        var proj = finalSpec.Projection;
        SortDefinition<TModel>? sort = null;
        foreach (var col in finalSpec.OrderBy)
        {
            if (col is Ascending<TModel> a)
            {
                if (sort == null)
                    sort = Builders<TModel>.Sort.Ascending(a.Expression);
                else
                    sort = sort.Ascending(a.Expression);
            }
            else if (col is Descending<TModel> b)
            {
                if (sort == null)
                    sort = Builders<TModel>.Sort.Descending(b.Expression);
                else
                    sort = sort.Descending(b.Expression);
            }
        }

        if (Session is null || Session.CurrentHandle is null)
            return await (await Collection.FindAsync<TProjection>(where, new FindOptions<TModel, TProjection>
            {
                Projection = Builders<TModel>.Projection.Expression<TProjection>(proj),
                Sort = sort,
                Limit = finalSpec.Limit,
                Skip = finalSpec.Skip
            }, cancellationToken: ct)).ToListAsync();
        else
            return await (await Collection.FindAsync<TProjection>(Session.CurrentHandle, where, new FindOptions<TModel, TProjection>
            {
                Projection = Builders<TModel>.Projection.Expression<TProjection>(proj),
                Sort = sort,
                Limit = finalSpec.Limit,
                Skip = finalSpec.Skip
            }, cancellationToken: ct)).ToListAsync();
    }

    public async Task<List<TModel>> FindManyByIdAsync(IEnumerable<ObjectId> ids, CancellationToken ct = default)
    {
        ApplyIsolationLevelFromSession();
        var idList = new List<ObjectId>(ids);
        var filter = Builders<TModel>.Filter.In("_id", idList);

        List<TModel> r;
        if (Session is null || Session.CurrentHandle is null)
            r = await (await Collection.FindAsync(filter, cancellationToken: ct)).ToListAsync();
        else
            r = await (await Collection.FindAsync(Session.CurrentHandle, filter, cancellationToken: ct)).ToListAsync();

        return r;
    }

    public async Task<TModel?> FindOneAsync(IFindSpecification<TModel> spec, CancellationToken ct = default)
    {
        ApplyIsolationLevelFromSession();
        var finalSpec = spec.GetSpec();
        var where = finalSpec.Predicate;

        TModel? r;
        if (Session is null || Session.CurrentHandle is null)
            r = await (await Collection.FindAsync(where, new FindOptions<TModel, TModel> { Limit = 1 }, cancellationToken: ct)).FirstOrDefaultAsync();
        else
            r = await (await Collection.FindAsync(Session.CurrentHandle, where, new FindOptions<TModel, TModel> { Limit = 1 }, cancellationToken: ct)).FirstOrDefaultAsync();

        return r;
    }

    public async Task<TProjection?> FindOneAsync<TProjection>(IFindSpecification<TModel, TProjection> spec, CancellationToken ct = default)
    {
        ApplyIsolationLevelFromSession();
        var finalSpec = spec.GetSpec();
        var where = finalSpec.Predicate;
        var proj = finalSpec.Projection;

        if (Session is null || Session.CurrentHandle is null)
            return await (await Collection.FindAsync<TProjection>(where, new FindOptions<TModel, TProjection> { Limit = 1, Projection = Builders<TModel>.Projection.Expression<TProjection>(proj) },
                cancellationToken: ct)).FirstOrDefaultAsync();
        else
            return await (await Collection.FindAsync<TProjection>(Session.CurrentHandle, where, new FindOptions<TModel, TProjection> { Limit = 1, Projection = Builders<TModel>.Projection.Expression<TProjection>(proj) },
                cancellationToken: ct)).FirstOrDefaultAsync();
    }

    public async Task<TModel?> FindOneByIdAsync(ObjectId id, CancellationToken ct = default)
    {
        ApplyIsolationLevelFromSession();
        var filter = Builders<TModel>.Filter.Eq("_id", id);

        TModel? r;
        if (Session is null || Session.CurrentHandle is null)
            r = await (await Collection.FindAsync(filter, new FindOptions<TModel, TModel> { Limit = 1 }, cancellationToken: ct)).FirstOrDefaultAsync();
        else
            r = await (await Collection.FindAsync(Session.CurrentHandle, filter, new FindOptions<TModel, TModel> { Limit = 1 }, cancellationToken: ct)).FirstOrDefaultAsync();

        return r;
    }

    public async Task InsertManyAsync(IEnumerable<TModel> items, CancellationToken ct = default)
    {
        ApplyIsolationLevelFromSession();
        if (Session is null || Session.CurrentHandle is null)
            await Collection.InsertManyAsync(items, cancellationToken: ct);
        else
            await Collection.InsertManyAsync(Session.CurrentHandle, items, cancellationToken: ct);

        foreach (var root in items)
        {
            await DispatchDomainEvents(root);
        }
    }

    public async Task InsertOneAsync(TModel item, CancellationToken ct = default)
    {
        ApplyIsolationLevelFromSession();
        if (Session is null || Session.CurrentHandle is null)
            await Collection.InsertOneAsync(item, cancellationToken: ct);
        else
            await Collection.InsertOneAsync(Session.CurrentHandle, item, cancellationToken: ct);

        await DispatchDomainEvents(item);
    }

    public async Task<bool> OneExistsAsync(ObjectId id, IFindSpecification<TModel>? predicate = null, CancellationToken ct = default)
    {
        ApplyIsolationLevelFromSession();
        Expression<Func<TModel, bool>> predicateExpression = x => x.Id == id;
        if (predicate != null)
        {
            var spec = predicate.GetSpec();
            predicateExpression = predicateExpression.AndAlso(spec.Predicate);
        }
        var projectionDef = Builders<TModel>.Projection.Expression(x => new ExistIdModel { Id = x.Id });
        ExistIdModel? r;
        if (Session is null || Session.CurrentHandle is null)
            r = await (await Collection.FindAsync(predicateExpression, new FindOptions<TModel, ExistIdModel> { Limit = 1, Projection = projectionDef },
                cancellationToken: ct)).FirstOrDefaultAsync();
        else
            r = await (await Collection.FindAsync(Session.CurrentHandle, predicateExpression, new FindOptions<TModel, ExistIdModel> { Limit = 1, Projection = projectionDef },
                cancellationToken: ct)).FirstOrDefaultAsync();
        return r != default(ExistIdModel);
    }

    public async Task<long> ReplaceOneAsync(TModel model, bool checkModified = true, CancellationToken ct = default)
    {
        ApplyIsolationLevelFromSession();
        var filter = Builders<TModel>.Filter.Eq("_id", model.Id);
        filter = Builders<TModel>.Filter.And(filter, Builders<TModel>.Filter.Eq("Version", model.LastVersion ?? model.Version));

        ReplaceOneResult replaceOneResult;
        model.IncVersion();
        if (Session is null || Session.CurrentHandle is null)
            replaceOneResult = await Collection.ReplaceOneAsync(filter, model, cancellationToken: ct);
        else
            replaceOneResult = await Collection.ReplaceOneAsync(Session.CurrentHandle, filter, model, cancellationToken: ct);

        var r = replaceOneResult.IsAcknowledged ? replaceOneResult.ModifiedCount : 0;
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
        ApplyIsolationLevelFromSession();
        var finalSpec = spec.GetSpec();
        var where = finalSpec.Predicate;
        var injector = new UpdateInjector<TModel>();
        foreach (var cmd in finalSpec.Commands)
            cmd.InjectField(injector);

        UpdateOptions? options = null;
        if (injector.ArrayFilters.Count != 0)
        {
            options = new UpdateOptions();
            options.ArrayFilters = injector.ArrayFilters;
        }

        var updateDefinition = injector.UpdateDefinition!;
        updateDefinition = updateDefinition.Inc(x => x.Version, 1);
        
        UpdateResult r;
        if (Session is null || Session.CurrentHandle is null)
            r = await Collection.UpdateManyAsync(where, updateDefinition, options: options, cancellationToken: ct);
        else
            r = await Collection.UpdateManyAsync(Session.CurrentHandle, where, updateDefinition, options: options, cancellationToken: ct);

        return r.IsAcknowledged ? r.ModifiedCount : 0;
    }

    public async Task<long> UpdateOneAsync(IUpdateSpecification<TModel> spec, CancellationToken ct = default)
    {
        ApplyIsolationLevelFromSession();
        var finalSpec = spec.GetSpec();
        var where = finalSpec.Predicate;
        var injector = new UpdateInjector<TModel>();
        foreach (var cmd in finalSpec.Commands)
            cmd.InjectField(injector);

        UpdateOptions? options = null;
        if (injector.ArrayFilters.Count != 0)
        {
            options = new UpdateOptions();
            options.ArrayFilters = injector.ArrayFilters;
        }
        
        var updateDefinition = injector.UpdateDefinition!;
        updateDefinition = updateDefinition.Inc(x => x.Version, 1);

        UpdateResult r;
        if (Session is null || Session.CurrentHandle is null)
            r = await Collection.UpdateOneAsync(where, updateDefinition, options: options, cancellationToken: ct);
        else
            r = await Collection.UpdateOneAsync(Session.CurrentHandle, where, updateDefinition, options: options, cancellationToken: ct);

        return r.IsAcknowledged ? r.ModifiedCount : 0;
    }

    public TSelf WithIsolation(IsolationLevel level)
    {
        var cloned = Clone(Session, SessionFactory);
        ((MongoRepository<TSelf, TModel>)(object)cloned).IsolationLevel = level;
        ((MongoRepository<TSelf, TModel>)(object)cloned).ApplyIsolationLevel(level);
        return cloned;
    }

    private IMongoCollection<T> ApplyIsolationLevel<T>(IMongoCollection<T> collection, IsolationLevel level)
    {
        switch (level)
        {
            case Abstractions.IsolationLevel.Uncommitted:
                collection = collection.WithReadConcern(ReadConcern.Local).WithWriteConcern(WriteConcern.WMajority).WithReadPreference(ReadPreference.Primary);
                break;
            case Abstractions.IsolationLevel.UncommittedStale:
                collection = collection.WithReadConcern(ReadConcern.Local).WithWriteConcern(WriteConcern.WMajority).WithReadPreference(ReadPreference.PrimaryPreferred);
                break;
            case Abstractions.IsolationLevel.UncommittedNearest:
                collection = collection.WithReadConcern(ReadConcern.Local).WithWriteConcern(WriteConcern.WMajority).WithReadPreference(ReadPreference.Nearest);
                break;
            case Abstractions.IsolationLevel.Committed:
                collection = collection.WithReadConcern(ReadConcern.Majority).WithWriteConcern(WriteConcern.WMajority).WithReadPreference(ReadPreference.Primary);
                break;
            case Abstractions.IsolationLevel.CommittedStale:
                collection = collection.WithReadConcern(ReadConcern.Majority).WithWriteConcern(WriteConcern.WMajority).WithReadPreference(ReadPreference.PrimaryPreferred);
                break;
            case Abstractions.IsolationLevel.CommittedNearest:
                collection = collection.WithReadConcern(ReadConcern.Majority).WithWriteConcern(WriteConcern.WMajority).WithReadPreference(ReadPreference.Nearest);
                break;
            case Abstractions.IsolationLevel.Linearizable:
                collection = collection.WithReadConcern(ReadConcern.Linearizable).WithWriteConcern(WriteConcern.WMajority).WithReadPreference(ReadPreference.Primary);
                break;
            case Abstractions.IsolationLevel.Snapshot:
                collection = collection.WithReadConcern(ReadConcern.Snapshot).WithWriteConcern(WriteConcern.WMajority).WithReadPreference(ReadPreference.Primary);
                break;
            default:
                break;
        }
        return collection;
    }

    private void ApplyIsolationLevel(IsolationLevel level)
    {
        switch (level)
        {
            case Abstractions.IsolationLevel.Uncommitted:
                this.Collection = this.Collection.WithReadConcern(ReadConcern.Local).WithWriteConcern(WriteConcern.WMajority).WithReadPreference(ReadPreference.Primary);
                break;
            case Abstractions.IsolationLevel.UncommittedStale:
                this.Collection = this.Collection.WithReadConcern(ReadConcern.Local).WithWriteConcern(WriteConcern.WMajority).WithReadPreference(ReadPreference.PrimaryPreferred);
                break;
            case Abstractions.IsolationLevel.UncommittedNearest:
                this.Collection = this.Collection.WithReadConcern(ReadConcern.Local).WithWriteConcern(WriteConcern.WMajority).WithReadPreference(ReadPreference.Nearest);
                break;
            case Abstractions.IsolationLevel.Committed:
                this.Collection = this.Collection.WithReadConcern(ReadConcern.Majority).WithWriteConcern(WriteConcern.WMajority).WithReadPreference(ReadPreference.Primary);
                break;
            case Abstractions.IsolationLevel.CommittedStale:
                this.Collection = this.Collection.WithReadConcern(ReadConcern.Majority).WithWriteConcern(WriteConcern.WMajority).WithReadPreference(ReadPreference.PrimaryPreferred);
                break;
            case Abstractions.IsolationLevel.CommittedNearest:
                this.Collection = this.Collection.WithReadConcern(ReadConcern.Majority).WithWriteConcern(WriteConcern.WMajority).WithReadPreference(ReadPreference.Nearest);
                break;
            case Abstractions.IsolationLevel.Linearizable:
                this.Collection = this.Collection.WithReadConcern(ReadConcern.Linearizable).WithWriteConcern(WriteConcern.WMajority).WithReadPreference(ReadPreference.Primary);
                break;
            case Abstractions.IsolationLevel.Snapshot:
                this.Collection = this.Collection.WithReadConcern(ReadConcern.Snapshot).WithWriteConcern(WriteConcern.WMajority).WithReadPreference(ReadPreference.Primary);
                break;
            default:
                break;
        }
    }

}

class ExistIdModel
{
    public ObjectId Id { get; set; }
}
