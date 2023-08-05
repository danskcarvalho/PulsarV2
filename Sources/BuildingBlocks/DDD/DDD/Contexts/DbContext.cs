namespace Pulsar.BuildingBlocks.DDD.Contexts;

public class DbContext
{
    private static AsyncLocal<Stack<DbContext>?> _context = new AsyncLocal<Stack<DbContext>?>();
    public static DbContext Current => (_context.Value?.Count != 0 ? _context.Value?.Peek() : null) ?? throw new InvalidOperationException("no db context");
    private List<IIsRepository> _repositories;
    private Dictionary<Type, object> _repositoriesPerModel = new Dictionary<Type, object>();

    internal static void SetContext(DbContext context)
    {
        if (_context.Value == null)
            _context.Value = new Stack<DbContext>();
        _context.Value.Push(context);

    }
    internal static void ClearContext()
    {
        if (_context.Value == null)
            throw new InvalidOperationException("no context");
        _context.Value.Pop();
        if (_context.Value.Count == 0)
            _context.Value = null;
    }


    public DbContext(IEnumerable<IIsRepository> repositories)
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
}
