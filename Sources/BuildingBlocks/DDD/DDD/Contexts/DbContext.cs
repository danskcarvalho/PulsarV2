namespace Pulsar.BuildingBlocks.DDD.Contexts;

public static class DbContext
{
    private static AsyncLocal<Stack<IDbContext>?> _context = new AsyncLocal<Stack<IDbContext>?>();
    public static IDbContext Current => (_context.Value?.Count != 0 ? _context.Value?.Peek() : null) ?? throw new InvalidOperationException("no db context");

    internal static void SetContext(IDbContext context)
    {
        if (_context.Value == null)
            _context.Value = new Stack<IDbContext>();
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
}
