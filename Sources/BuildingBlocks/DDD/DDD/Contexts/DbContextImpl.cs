using DDD.Contracts;

namespace Pulsar.BuildingBlocks.DDD.Contexts;

public class DbContextImpl : IDbContext
{
    private static AsyncLocal<Stack<DbContextImpl>?> _context = new AsyncLocal<Stack<DbContextImpl>?>();
    public static DbContextImpl Current => (_context.Value?.Count != 0 ? _context.Value?.Peek() : null) ?? throw new InvalidOperationException("no db context");
    private List<IIsRepository> _repositories;
    private Dictionary<Type, object> _repositoriesPerModel = new Dictionary<Type, object>();

    internal static void SetContext(DbContextImpl contextImpl)
    {
        if (_context.Value == null)
            _context.Value = new Stack<DbContextImpl>();
        _context.Value.Push(contextImpl);

    }
    internal static void ClearContext()
    {
        if (_context.Value == null)
            throw new InvalidOperationException("no context");
        _context.Value.Pop();
        if (_context.Value.Count == 0)
            _context.Value = null;
    }


    public DbContextImpl(IEnumerable<IIsRepository> repositories)
    {
        _repositories = repositories.ToList();
    }

    public DbContextCollection<TModel> GetCollection<TModel>() where TModel : class, IAggregateRoot
    {
        if (_repositoriesPerModel.ContainsKey(typeof(TModel)))
            return (DbContextCollection<TModel>)_repositoriesPerModel[typeof(TModel)];

        var repository = _repositories.First(r => r is IRepositoryBase<TModel>);
        var collection = new DbContextCollection<TModel>((IRepositoryBase<TModel>)repository);
        _repositoriesPerModel[typeof(TModel)] = collection;
        return collection;
    }

    Task<bool> IDbContext.Exists<TModel>(ObjectId id)
    {
        return GetCollection<TModel>().Exists(id);
    }

    Task<bool> IDbContext.Exists<TModel>(params ObjectId[] ids)
    {
        return GetCollection<TModel>().Exists(ids);
    }

    Task<bool> IDbContext.Exists<TModel>(IEnumerable<ObjectId> ids)
    {
        return GetCollection<TModel>().Exists(ids);
    }

    Task<TModel?> IDbContext.TryGet<TModel>(ObjectId id) where TModel : class
    {
        return GetCollection<TModel>().Get(id);
    }

    async Task<TModel> IDbContext.Get<TModel>(ObjectId id)
    {
        return await GetCollection<TModel>().Get(id) ??
               throw new InvalidOperationException($"no entity of type {typeof(TModel).Name} with _id {id}");
    }

    async Task<TModel> IDbContext.GetAndCache<TModel>(ObjectId id, string key)
    {
        return await GetCollection<TModel>().GetAndCache(id, key) ??
            throw new InvalidOperationException($"no entity of type {typeof(TModel).Name} with _id {id}");
    }

    Task<TModel?> IDbContext.TryGetAndCache<TModel>(ObjectId id, string key) where TModel : class
    {
        return GetCollection<TModel>().GetAndCache(id, key);
    }

    Task<List<TModel>> IDbContext.GetMany<TModel>(IEnumerable<ObjectId> ids)
    {
        return GetCollection<TModel>().Get(ids);
    }
}
