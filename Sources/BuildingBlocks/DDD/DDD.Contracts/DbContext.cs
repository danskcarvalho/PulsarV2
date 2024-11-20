using System.Linq.Expressions;
using System.Reflection;

namespace DDD.Contracts;

public class DbContext
{
    private static Func<IDbContext>? _getDbContext = null;

    public static IDbContext Current
    {
        get
        {
            InitGetDbContext();
            return _getDbContext?.Invoke()!;
        }
    }

    private static void InitGetDbContext()
    {
        if (_getDbContext == null)
        {
            var type = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .First(t => typeof(IDbContext).IsAssignableFrom(t));
            var prop = type.GetProperty("Current", BindingFlags.Public | BindingFlags.Static)!;
            var propExp = Expression.Property(null, prop);
            _getDbContext = Expression.Lambda<Func<IDbContext>>(propExp).Compile();
        }
    }
}