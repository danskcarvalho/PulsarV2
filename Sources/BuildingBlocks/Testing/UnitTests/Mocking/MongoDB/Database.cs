namespace Pulsar.BuildingBlocks.UnitTests.Mocking.MongoDB;

class Database : IMockedDatabase
{
    private readonly Stack<Dictionary<string, Collection>> _collections = new Stack<Dictionary<string, Collection>>();
    private readonly Dictionary<string, object> _collectionWrappers = new Dictionary<string, object>();

    bool IMockedDatabase.IsInTransaction => _collections.Count > 1;

    public Database()
    {
        _collections.Push(new Dictionary<string, Collection>());
    }

    public Database(Stack<Dictionary<string, Collection>> collections)
    {
        _collections = collections;
    }

    internal Collection<T> GetUnderlyingCollection<T>(string collectionName) where T : class
    {
        var current = _collections.Peek();
        if (!current.ContainsKey(collectionName))
        {
            current[collectionName] = new Collection<T>();
        }

        return (Collection<T>)current[collectionName];
    }

    public CollectionWrapper<T> GetCollection<T>(string collectionName) where T : class
    {
        if (!_collectionWrappers.ContainsKey(collectionName))
        {
            _collectionWrappers[collectionName] = new CollectionWrapper<T>(this, collectionName);
        }
        return (CollectionWrapper<T>)_collectionWrappers[collectionName];
    }

    async Task<TResult> OpenTransaction<TResult>(Func<Task<TResult>> action)
    {
        PushFrame();
        try
        {
            var r = await action();
            PopAndCommitFrame();
            return r;
        }
        catch
        {
            PopAndDiscardFrame();
            throw;
        }
    }

    private void PopAndCommitFrame()
    {
        var n = _collections.Pop();
        _collections.Pop();
        _collections.Push(n);
    }

    private void PopAndDiscardFrame()
    {
        _collections.Pop();
    }

    private void PushFrame()
    {
        var p = _collections.Peek();
        var n = new Dictionary<string, Collection>();
        foreach (var kvp in p)
        {
            n[kvp.Key] = kvp.Value.Clone();
        }
        _collections.Push(n);
    }

    Task<TResult> IMockedDatabase.WithTransactionAsync<TResult>(Func<Task<TResult>> action)
    {
        return OpenTransaction<TResult>(action);
    }

    IMockedCollection<T> IMockedDatabase.GetCollection<T>(string collectionName)
    {
        return GetCollection<T>(collectionName);
    }

    public IMockedDatabase Clone()
    {
        var newCollections = new Stack<Dictionary<string, Collection>>();
        foreach (var dic in _collections.Reverse())
        {
            var newDic = new Dictionary<string, Collection>();
            foreach (var kvp in dic)
            {
                newDic[kvp.Key] = kvp.Value.Clone();
            }
            newCollections.Push(newDic);
        }

        return new Database(newCollections);
    }
}
