using MongoDB.Driver;
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

    public MongoRepository(MongoDbSession? session, MongoDbSessionFactory sessionFactory)
    {
        Session = session;
        SessionFactory = sessionFactory;
        Collection = sessionFactory.Database.GetCollection<TModel>(CollectionName);
        if (!ApplyIsolationLevelFromSession())
            Collection = Collection.WithWriteConcern(WriteConcern.WMajority).WithReadConcern(ReadConcern.Majority).WithReadPreference(ReadPreference.Primary);
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

    protected abstract string CollectionName { get; }
    public void Track(TModel? model)
    {
        if (model == null)
            return;
        if (Session is not null)
            Session.TrackAggregateRoot(model);
    }
    protected abstract TSelf Clone(MongoDbSession? session, MongoDbSessionFactory sessionFactory);

    public async Task<bool> AllExistsAsync(IEnumerable<ObjectId> ids, CancellationToken ct = default)
    {
        ApplyIsolationLevelFromSession();
        var idList = ids.ToList();
        var filter = Builders<TModel>.Filter.In("_id", idList);
        var projectionDef = Builders<TModel>.Projection.Include("_id");
        List<TModel> r;

        if (Session is null || Session.CurrentHandle is null)
            r = await (await Collection.FindAsync(filter, new FindOptions<TModel> { Projection = projectionDef },
                cancellationToken: ct)).ToListAsync();
        else
            r = await (await Collection.FindAsync(Session.CurrentHandle, filter, new FindOptions<TModel> { Projection = projectionDef },
                cancellationToken: ct)).ToListAsync();

        HashSet<ObjectId> found = new HashSet<ObjectId>();
        found.UnionWith(r.Select(x => (ObjectId)x.GetType().GetProperty("Id")!.GetValue(x)!));
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
    public async Task<long> DeleteOneAsync(TModel model, long? version = null, CancellationToken ct = default)
    {
        var r = await DeleteOneByIdAsync(model.Id, version, ct);
        Track(model);
        return r;
    }

    public TSelf EscapeSession()
    {
        var cloned = Clone(null, SessionFactory);
        return cloned;
    }

    public async Task<List<TModel>> FindManyAsync(IFindSpecification<TModel> spec, bool noTracking = false, CancellationToken ct = default)
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

        if (!noTracking)
        {
            foreach (var root in r)
                Track(root);
        }

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

    public async Task<List<TModel>> FindManyByIdAsync(IEnumerable<ObjectId> ids, bool noTracking = false, CancellationToken ct = default)
    {
        ApplyIsolationLevelFromSession();
        var idList = new List<ObjectId>(ids);
        var filter = Builders<TModel>.Filter.In("_id", idList);

        List<TModel> r;
        if (Session is null || Session.CurrentHandle is null)
            r = await (await Collection.FindAsync(filter, cancellationToken: ct)).ToListAsync();
        else
            r = await (await Collection.FindAsync(Session.CurrentHandle, filter, cancellationToken: ct)).ToListAsync();

        if (!noTracking)
        {
            foreach (var root in r)
                Track(root);
        }

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

        Track(r);
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

        Track(r);
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
            Track(root);
    }

    public async Task InsertOneAsync(TModel item, CancellationToken ct = default)
    {
        ApplyIsolationLevelFromSession();
        if (Session is null || Session.CurrentHandle is null)
            await Collection.InsertOneAsync(item, cancellationToken: ct);
        else
            await Collection.InsertOneAsync(Session.CurrentHandle, item, cancellationToken: ct);

        Track(item);
    }

    public async Task<bool> OneExistsAsync(ObjectId id, CancellationToken ct = default)
    {
        ApplyIsolationLevelFromSession();
        var filter = Builders<TModel>.Filter.Eq("_id", id);
        var projectionDef = Builders<TModel>.Projection.Include("_id");
        TModel? r;
        if (Session is null || Session.CurrentHandle is null)
            r = await (await Collection.FindAsync(filter, new FindOptions<TModel> { Limit = 1, Projection = projectionDef },
                cancellationToken: ct)).FirstOrDefaultAsync();
        else
            r = await (await Collection.FindAsync(Session.CurrentHandle, filter, new FindOptions<TModel> { Limit = 1, Projection = projectionDef },
                cancellationToken: ct)).FirstOrDefaultAsync();
        return r != default(TModel);
    }

    public async Task<long> ReplaceOneAsync(TModel model, long? version = null, CancellationToken ct = default)
    {
        ApplyIsolationLevelFromSession();
        var filter = Builders<TModel>.Filter.Eq("_id", model.Id);
        if (version.HasValue)
            filter = Builders<TModel>.Filter.And(filter, Builders<TModel>.Filter.Eq("Version", version));

        ReplaceOneResult r;
        if (Session is null || Session.CurrentHandle is null)
            r = await Collection.ReplaceOneAsync(filter, model, cancellationToken: ct);
        else
            r = await Collection.ReplaceOneAsync(Session.CurrentHandle, filter, model, cancellationToken: ct);

        var modified = r.IsAcknowledged ? r.ModifiedCount : 0;
        Track(model);
        return modified;
    }

    public async Task<long> UpdateManyAsync(IUpdateSpecification<TModel> spec, CancellationToken ct = default)
    {
        ApplyIsolationLevelFromSession();
        var finalSpec = spec.GetSpec();
        var where = finalSpec.Predicate;
        var injector = new UpdateInjector<TModel>();
        foreach (var cmd in finalSpec.Commands)
            cmd.InjectField(injector);

        UpdateResult r;
        if (Session is null || Session.CurrentHandle is null)
            r = await Collection.UpdateManyAsync(where, injector.UpdateDefinition!, cancellationToken: ct);
        else
            r = await Collection.UpdateManyAsync(Session.CurrentHandle, where, injector.UpdateDefinition!, cancellationToken: ct);

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

        UpdateResult r;
        if (Session is null || Session.CurrentHandle is null)
            r = await Collection.UpdateOneAsync(where, injector.UpdateDefinition!, cancellationToken: ct);
        else
            r = await Collection.UpdateOneAsync(Session.CurrentHandle, where, injector.UpdateDefinition!, cancellationToken: ct);

        return r.IsAcknowledged ? r.ModifiedCount : 0;
    }

    public TSelf WithIsolation(IsolationLevel level)
    {
        var cloned = Clone(Session, SessionFactory);
        ((MongoRepository<TSelf, TModel>)(object)cloned).IsolationLevel = level;
        ((MongoRepository<TSelf, TModel>)(object)cloned).ApplyIsolationLevel(level);
        return cloned;
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

class UpdateInjector<T> : IUpdateInjectField<T>
{
    public UpdateDefinition<T>? UpdateDefinition { get; set; }

    public void Inject<TField>(string commandName, Expression<Func<T, TField>> expression, TField value)
    {
        if (UpdateDefinition == null)
        {
            switch (commandName)
            {
                case UpdateCommandNames.Inc:
                    UpdateDefinition = Builders<T>.Update.Inc(expression, value);
                    break;
                case UpdateCommandNames.AddToSet:
                    break;
                case UpdateCommandNames.PopFirst:
                    break;
                case UpdateCommandNames.PopLast:
                    break;
                case UpdateCommandNames.Push:
                    break;
                case UpdateCommandNames.Set:
                    UpdateDefinition = Builders<T>.Update.Set(expression, value);
                    break;
                case UpdateCommandNames.Unset:
                    break;
                case UpdateCommandNames.AddToSetEach:
                    break;
                case UpdateCommandNames.Max:
                    UpdateDefinition = Builders<T>.Update.Max(expression, value);
                    break;
                case UpdateCommandNames.Min:
                    UpdateDefinition = Builders<T>.Update.Min(expression, value);
                    break;
                case UpdateCommandNames.Mul:
                    UpdateDefinition = Builders<T>.Update.Mul(expression, value);
                    break;
                case UpdateCommandNames.Pull:
                    break;
                case UpdateCommandNames.PullAll:
                    break;
                case UpdateCommandNames.PullFilter:
                    break;
                case UpdateCommandNames.PushEach:
                    break;
                default:
                    break;
            }
        }
        else
        {
            switch (commandName)
            {
                case UpdateCommandNames.Inc:
                    UpdateDefinition = UpdateDefinition.Inc(expression, value);
                    break;
                case UpdateCommandNames.AddToSet:
                    break;
                case UpdateCommandNames.PopFirst:
                    break;
                case UpdateCommandNames.PopLast:
                    break;
                case UpdateCommandNames.Push:
                    break;
                case UpdateCommandNames.Set:
                    UpdateDefinition = UpdateDefinition.Set(expression, value);
                    break;
                case UpdateCommandNames.Unset:
                    break;
                case UpdateCommandNames.AddToSetEach:
                    break;
                case UpdateCommandNames.Max:
                    UpdateDefinition = UpdateDefinition.Max(expression, value);
                    break;
                case UpdateCommandNames.Min:
                    UpdateDefinition = UpdateDefinition.Min(expression, value);
                    break;
                case UpdateCommandNames.Mul:
                    UpdateDefinition = UpdateDefinition.Mul(expression, value);
                    break;
                case UpdateCommandNames.Pull:
                    break;
                case UpdateCommandNames.PullAll:
                    break;
                case UpdateCommandNames.PullFilter:
                    break;
                case UpdateCommandNames.PushEach:
                    break;
                default:
                    break;
            }
        }
    }

    public void Inject(string commandName, Expression<Func<T, object>> expression)
    {
        if (UpdateDefinition == null)
        {
            switch (commandName)
            {
                case UpdateCommandNames.Inc:
                    break;
                case UpdateCommandNames.AddToSet:
                    break;
                case UpdateCommandNames.PopFirst:
                    UpdateDefinition = Builders<T>.Update.PopFirst(expression);
                    break;
                case UpdateCommandNames.PopLast:
                    UpdateDefinition = Builders<T>.Update.PopLast(expression);
                    break;
                case UpdateCommandNames.Push:
                    break;
                case UpdateCommandNames.Set:
                    break;
                case UpdateCommandNames.Unset:
                    UpdateDefinition = Builders<T>.Update.Unset(expression);
                    break;
                case UpdateCommandNames.AddToSetEach:
                    break;
                case UpdateCommandNames.Max:
                    break;
                case UpdateCommandNames.Min:
                    break;
                case UpdateCommandNames.Mul:
                    break;
                case UpdateCommandNames.Pull:
                    break;
                case UpdateCommandNames.PullAll:
                    break;
                case UpdateCommandNames.PullFilter:
                    break;
                case UpdateCommandNames.PushEach:
                    break;
                default:
                    break;
            }
        }
        else
        {
            switch (commandName)
            {
                case UpdateCommandNames.Inc:
                    break;
                case UpdateCommandNames.AddToSet:
                    break;
                case UpdateCommandNames.PopFirst:
                    UpdateDefinition = UpdateDefinition.PopFirst(expression);
                    break;
                case UpdateCommandNames.PopLast:
                    UpdateDefinition = UpdateDefinition.PopLast(expression);
                    break;
                case UpdateCommandNames.Push:
                    break;
                case UpdateCommandNames.Set:
                    break;
                case UpdateCommandNames.Unset:
                    UpdateDefinition = UpdateDefinition.Unset(expression);
                    break;
                case UpdateCommandNames.AddToSetEach:
                    break;
                case UpdateCommandNames.Max:
                    break;
                case UpdateCommandNames.Min:
                    break;
                case UpdateCommandNames.Mul:
                    break;
                case UpdateCommandNames.Pull:
                    break;
                case UpdateCommandNames.PullAll:
                    break;
                case UpdateCommandNames.PullFilter:
                    break;
                case UpdateCommandNames.PushEach:
                    break;
                default:
                    break;
            }
        }
    }

    public void Inject<TField>(string commandName, Expression<Func<T, IEnumerable<TField>>> expression, TField value)
    {
        if (UpdateDefinition == null)
        {
            switch (commandName)
            {
                case UpdateCommandNames.Inc:
                    break;
                case UpdateCommandNames.AddToSet:
                    UpdateDefinition = Builders<T>.Update.AddToSet(expression, value);
                    break;
                case UpdateCommandNames.PopFirst:
                    break;
                case UpdateCommandNames.PopLast:
                    break;
                case UpdateCommandNames.Push:
                    UpdateDefinition = Builders<T>.Update.Push(expression, value);
                    break;
                case UpdateCommandNames.Set:
                    break;
                case UpdateCommandNames.Unset:
                    break;
                case UpdateCommandNames.AddToSetEach:
                    break;
                case UpdateCommandNames.Max:
                    break;
                case UpdateCommandNames.Min:
                    break;
                case UpdateCommandNames.Mul:
                    break;
                case UpdateCommandNames.Pull:
                    UpdateDefinition = Builders<T>.Update.Pull(expression, value);
                    break;
                case UpdateCommandNames.PullAll:
                    break;
                case UpdateCommandNames.PullFilter:
                    break;
                case UpdateCommandNames.PushEach:
                    break;
                default:
                    break;
            }
        }
        else
        {
            switch (commandName)
            {
                case UpdateCommandNames.Inc:
                    break;
                case UpdateCommandNames.AddToSet:
                    UpdateDefinition = UpdateDefinition.AddToSet(expression, value);
                    break;
                case UpdateCommandNames.PopFirst:
                    break;
                case UpdateCommandNames.PopLast:
                    break;
                case UpdateCommandNames.Push:
                    UpdateDefinition = UpdateDefinition.Push(expression, value);
                    break;
                case UpdateCommandNames.Set:
                    break;
                case UpdateCommandNames.Unset:
                    break;
                case UpdateCommandNames.AddToSetEach:
                    break;
                case UpdateCommandNames.Max:
                    break;
                case UpdateCommandNames.Min:
                    break;
                case UpdateCommandNames.Mul:
                    break;
                case UpdateCommandNames.Pull:
                    UpdateDefinition = UpdateDefinition.Pull(expression, value);
                    break;
                case UpdateCommandNames.PullAll:
                    break;
                case UpdateCommandNames.PullFilter:
                    break;
                case UpdateCommandNames.PushEach:
                    break;
                default:
                    break;
            }
        }
    }

    public void Inject<TField>(string commandName, Expression<Func<T, IEnumerable<TField>>> expression, IEnumerable<TField> values)
    {
        if (UpdateDefinition == null)
        {
            switch (commandName)
            {
                case UpdateCommandNames.Inc:
                    break;
                case UpdateCommandNames.AddToSet:
                    break;
                case UpdateCommandNames.PopFirst:
                    break;
                case UpdateCommandNames.PopLast:
                    break;
                case UpdateCommandNames.Push:
                    break;
                case UpdateCommandNames.Set:
                    break;
                case UpdateCommandNames.Unset:
                    break;
                case UpdateCommandNames.AddToSetEach:
                    UpdateDefinition = Builders<T>.Update.AddToSetEach(expression, values);
                    break;
                case UpdateCommandNames.Max:
                    break;
                case UpdateCommandNames.Min:
                    break;
                case UpdateCommandNames.Mul:
                    break;
                case UpdateCommandNames.Pull:
                    break;
                case UpdateCommandNames.PullAll:
                    UpdateDefinition = Builders<T>.Update.PullAll(expression, values);
                    break;
                case UpdateCommandNames.PullFilter:
                    break;
                case UpdateCommandNames.PushEach:
                    UpdateDefinition = Builders<T>.Update.PushEach(expression, values);
                    break;
                default:
                    break;
            }
        }
        else
        {
            switch (commandName)
            {
                case UpdateCommandNames.Inc:
                    break;
                case UpdateCommandNames.AddToSet:
                    break;
                case UpdateCommandNames.PopFirst:
                    break;
                case UpdateCommandNames.PopLast:
                    break;
                case UpdateCommandNames.Push:
                    break;
                case UpdateCommandNames.Set:
                    break;
                case UpdateCommandNames.Unset:
                    break;
                case UpdateCommandNames.AddToSetEach:
                    UpdateDefinition = UpdateDefinition.AddToSetEach(expression, values);
                    break;
                case UpdateCommandNames.Max:
                    break;
                case UpdateCommandNames.Min:
                    break;
                case UpdateCommandNames.Mul:
                    break;
                case UpdateCommandNames.Pull:
                    break;
                case UpdateCommandNames.PullAll:
                    UpdateDefinition = UpdateDefinition.PullAll(expression, values);
                    break;
                case UpdateCommandNames.PullFilter:
                    break;
                case UpdateCommandNames.PushEach:
                    UpdateDefinition = UpdateDefinition.PushEach(expression, values);
                    break;
                default:
                    break;
            }
        }
    }

    public void Inject<TField>(string commandName, Expression<Func<T, IEnumerable<TField>>> expression, Expression<Func<TField, bool>> filter)
    {
        if (UpdateDefinition == null)
        {
            switch (commandName)
            {
                case UpdateCommandNames.Inc:
                    break;
                case UpdateCommandNames.AddToSet:
                    break;
                case UpdateCommandNames.PopFirst:
                    break;
                case UpdateCommandNames.PopLast:
                    break;
                case UpdateCommandNames.Push:
                    break;
                case UpdateCommandNames.Set:
                    break;
                case UpdateCommandNames.Unset:
                    break;
                case UpdateCommandNames.AddToSetEach:
                    break;
                case UpdateCommandNames.Max:
                    break;
                case UpdateCommandNames.Min:
                    break;
                case UpdateCommandNames.Mul:
                    break;
                case UpdateCommandNames.Pull:
                    break;
                case UpdateCommandNames.PullAll:
                    break;
                case UpdateCommandNames.PullFilter:
                    UpdateDefinition = Builders<T>.Update.PullFilter(expression, filter);
                    break;
                case UpdateCommandNames.PushEach:
                    break;
                default:
                    break;
            }
        }
        else
        {
            switch (commandName)
            {
                case UpdateCommandNames.Inc:
                    break;
                case UpdateCommandNames.AddToSet:
                    break;
                case UpdateCommandNames.PopFirst:
                    break;
                case UpdateCommandNames.PopLast:
                    break;
                case UpdateCommandNames.Push:
                    break;
                case UpdateCommandNames.Set:
                    break;
                case UpdateCommandNames.Unset:
                    break;
                case UpdateCommandNames.AddToSetEach:
                    break;
                case UpdateCommandNames.Max:
                    break;
                case UpdateCommandNames.Min:
                    break;
                case UpdateCommandNames.Mul:
                    break;
                case UpdateCommandNames.Pull:
                    break;
                case UpdateCommandNames.PullAll:
                    break;
                case UpdateCommandNames.PullFilter:
                    UpdateDefinition = UpdateDefinition.PullFilter(expression, filter);
                    break;
                case UpdateCommandNames.PushEach:
                    break;
                default:
                    break;
            }
        }
    }
}
