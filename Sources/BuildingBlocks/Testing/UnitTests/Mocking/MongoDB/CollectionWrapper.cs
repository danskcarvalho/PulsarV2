namespace Pulsar.BuildingBlocks.UnitTests.Mocking.MongoDB;

class CollectionWrapper<T> : IMockedCollection<T> where T : class
{
    private readonly Database _database;
    private readonly string _collectionName;

    public CollectionWrapper(Database database, string collectionName)
    {
        _database = database;
        _collectionName = collectionName;
    }

    public void AddUniqueKey<TKey>(Func<T, TKey> keyExtractor) where TKey : notnull
    {
        var _col = _database.GetUnderlyingCollection<T>(_collectionName);
        _col.AddUniqueKey(keyExtractor);
    }

    public void InsertMany(IEnumerable<T> items)
    {
        var _col = _database.GetUnderlyingCollection<T>(_collectionName);
        _col.InsertMany(items);
    }

    public T? FindById(ObjectId id)
    {
        var _col = _database.GetUnderlyingCollection<T>(_collectionName);
        return _col.FindById(id);
    }

    public IEnumerable<T> Find(IFindSpecification<T> specification)
    {
        var _col = _database.GetUnderlyingCollection<T>(_collectionName);
        return _col.Find(specification);
    }

    public IEnumerable<TProjection> Find<TProjection>(IFindSpecification<T, TProjection> specification)
    {
        var _col = _database.GetUnderlyingCollection<T>(_collectionName);
        return _col.Find(specification);
    }

    public long DeleteMany(IDeleteSpecification<T> specification, long? limit = null)
    {
        var _col = _database.GetUnderlyingCollection<T>(_collectionName);
        return _col.DeleteMany(specification, limit);
    }

    public long UpdateMany(IUpdateSpecification<T> specification, long? limit = null)
    {
        var _col = _database.GetUnderlyingCollection<T>(_collectionName);
        return _col.UpdateMany(specification, limit);
    }

    public bool Exists(ObjectId id)
    {
        var _col = _database.GetUnderlyingCollection<T>(_collectionName);
        return _col.Exists(id);
    }

    public long Replace(T item, ObjectId id, long? version = null, string? stringPropertyName = null)
    {
        var _col = _database.GetUnderlyingCollection<T>(_collectionName);
        return _col.Replace(item, id, version, stringPropertyName);
    }

    Task IMockedCollection<T>.InsertManyAsync(IEnumerable<T> items)
    {
        InsertMany(items);
        return Task.CompletedTask;
    }

    Task<T?> IMockedCollection<T>.FindByIdAsync(ObjectId id)
    {
        return Task.FromResult(FindById(id));
    }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    async IAsyncEnumerable<T> IMockedCollection<T>.FindAsync(IFindSpecification<T> specification)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    {
        foreach (var item in Find(specification))
        {
            yield return item;
        }
    }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    async IAsyncEnumerable<TProjection> IMockedCollection<T>.FindAsync<TProjection>(IFindSpecification<T, TProjection> specification)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    {
        foreach (var item in Find<TProjection>(specification))
        {
            yield return item;
        }
    }

    Task<long> IMockedCollection<T>.DeleteManyAsync(IDeleteSpecification<T> specification, long? limit)
    {
        return Task.FromResult(DeleteMany(specification, limit));
    }

    Task<long> IMockedCollection<T>.UpdateManyAsync(IUpdateSpecification<T> specification, long? limit)
    {
        return Task.FromResult(UpdateMany(specification, limit));
    }

    Task<bool> IMockedCollection<T>.ExistsAsync(ObjectId id)
    {
        return Task.FromResult(Exists(id));
    }

    Task<long> IMockedCollection<T>.ReplaceAsync(T item, ObjectId id, long? version, string? stringPropertyName)
    {
        return Task.FromResult(Replace(item, id, version, stringPropertyName));
    }

    Task IMockedCollection<T>.AddUniqueKey<TKey>(Func<T, TKey> keyExtractor)
    {
        AddUniqueKey(keyExtractor);
        return Task.CompletedTask;
    }
}
