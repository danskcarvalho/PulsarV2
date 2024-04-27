using System.Reflection;

namespace Pulsar.BuildingBlocks.Sync.Domain;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class TrackerAttribute : Attribute
{
    public static IEnumerable<Type> GetTrackerTypes(params Assembly[] assembliesToScan)
    {
        return assembliesToScan.SelectMany(a => a.GetTypes())
            .Where(x => x.GetCustomAttribute<TrackerAttribute>() != null &&
                        typeof(Tracker).IsAssignableFrom(x));
    }
}
