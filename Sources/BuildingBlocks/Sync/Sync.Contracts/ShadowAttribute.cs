using System.Reflection;

namespace Pulsar.BuildingBlocks.Sync.Contracts;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class ShadowAttribute : Attribute
{
    public string Name { get; set; }

    public ShadowAttribute(string name)
    {
        Name = name;
    }

    public static IEnumerable<(Type Type, ShadowAttribute Attribute)> GetShadowTypes(params Assembly[] assembliesToScan)
    {
        return assembliesToScan.SelectMany(a => a.GetTypes())
            .Where(x => x.GetCustomAttribute<ShadowAttribute>() != null)
            .Select(x => (x, x.GetCustomAttribute<ShadowAttribute>()!));
    }
}
