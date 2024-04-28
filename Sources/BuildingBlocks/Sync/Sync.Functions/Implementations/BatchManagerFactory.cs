using System.Reflection;
using Pulsar.BuildingBlocks.Sync.Contracts;
using Pulsar.BuildingBlocks.Sync.Domain;
using Pulsar.BuildingBlocks.Sync.Functions.Abstractions;

namespace Pulsar.BuildingBlocks.Sync.Functions.Implementations;

class BatchManagerFactory : IBatchManagerFactory
{
    private readonly Dictionary<string, Type> _shadowTypes = new Dictionary<string, Type>();
    private readonly Dictionary<Type, object> _managersForShadows = new Dictionary<Type, object>();
    private readonly List<(Type Type, Type TrackedEntity)> _trackers = [];
    
    public BatchManagerFactory()
    {
        MapShadowTypes();
        CacheTrackers();
        CacheManagersForShadows();
    }
    public IEnumerable<IBatchManagerForEvent> GetManagersFromEvent(EntityChangedIE evt, object? originalShadow)
    {
        var shadowName = evt.ShadowName;
        var shadowType = GetShadowType(shadowName);
        return GetManagersForShadow(shadowType, evt, originalShadow);
    }

    private Type GetShadowType(string shadowName)
    {
        return _shadowTypes[shadowName];
    }

    private void MapShadowTypes()
    {
        foreach (var item in ShadowAttribute.GetShadowTypes(AppDomain.CurrentDomain.GetAssemblies()))
        {
            _shadowTypes[item.Attribute.Name] = item.Type;
        }
    }

    private void CacheTrackers()
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach (var type in assembly.GetTypes())
            {
                if (typeof(Tracker).IsAssignableFrom(type) && type.GetCustomAttribute<TrackerAttribute>() != null)
                {
                    _trackers.Add((type, GetTrackedEntity(type)));
                }
            }
        }
    }

    private Type GetTrackedEntity(Type type)
    {
        if (type.BaseType == null)
        {
            throw new InvalidOperationException();
        }

        if (!type.BaseType.IsConstructedGenericType)
        {
            throw new InvalidOperationException($"must inherit from Tracker<>");
        }

        var def = type.BaseType.GetGenericTypeDefinition();
        
        if (def != typeof(Tracker<>))
        {
            throw new InvalidOperationException($"must inherit from Tracker<>");
        }

        return type.GetGenericArguments()[0];
    }

    private void CacheManagersForShadows()
    {
        foreach (var shadow in _shadowTypes)
        {
            _managersForShadows[shadow.Value] = CreateBatchManagerForShadow(shadow.Value, shadow.Key);
        }
    }

    private object CreateBatchManagerForShadow(Type shadowType, string shadowName)
    {
        var cls = typeof(BatchManagerForShadow<>).MakeGenericType(shadowType);
        return Activator.CreateInstance(cls, [shadowType, shadowName, _trackers.ToList()]) ?? throw new InvalidOperationException();
    }

    private IEnumerable<IBatchManagerForEvent> GetManagersForShadow(Type shadowType, EntityChangedIE evt, object? originalShadow)
    {
        var manager = (IBatchManagerForShadow)_managersForShadows[shadowType];
        foreach (var tracker in _trackers)
        {
            foreach (var entManager in manager.GetManagersForEntity(tracker.TrackedEntity, evt, originalShadow))
            {
                yield return entManager;
            }
        }
    }
}