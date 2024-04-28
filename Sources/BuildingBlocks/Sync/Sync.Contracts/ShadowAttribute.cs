using System.Reflection;

namespace Pulsar.BuildingBlocks.Sync.Contracts;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class ShadowAttribute : Attribute
{
    public string Name { get; }

    public ShadowAttribute(string name)
    {
        Name = name;
    }

    public static IEnumerable<(Type Type, ShadowAttribute Attribute)> GetShadowTypes(params Assembly[] assembliesToScan)
    {
        return assembliesToScan.SelectMany(a => a.GetTypes())
            .Where(x => x.GetCustomAttribute<ShadowAttribute>() != null)
            .Select(x => (ValidateShadowType(x), x.GetCustomAttribute<ShadowAttribute>()!));
    }

    private static Type ValidateShadowType(Type type)
    {
        if (!typeof(IShadow).IsAssignableFrom(type))
        {
            throw new InvalidOperationException($"type {type.FullName} does not implement IShadow");
        }

        return type;
    }
}
