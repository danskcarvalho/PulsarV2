using System.Reflection;
using Pulsar.BuildingBlocks.DDD.Abstractions;
using Pulsar.BuildingBlocks.Sync.Contracts;
using Pulsar.BuildingBlocks.Sync.Domain;
using Pulsar.BuildingBlocks.Sync.Functions.Abstractions;
using Pulsar.BuildingBlocks.Sync.Functions.Entities;
using Pulsar.BuildingBlocks.Utils;

namespace Pulsar.BuildingBlocks.Sync.Functions.Implementations;

class BatchManagerForShadow<TShadow> : IBatchManagerForShadow where TShadow : class, IShadow
{
    private readonly Type _shadowType;
    private readonly string _shadowName;
    private readonly List<(Type Type, Type TrackedEntity)> _trackers;
    private readonly IBatchDbContextFactory _factory;

    private readonly Dictionary<Type, List<IBatchManagerForShadowAndEntity<TShadow>>> _managers =
        new Dictionary<Type, List<IBatchManagerForShadowAndEntity<TShadow>>>();

    public BatchManagerForShadow(IBatchDbContextFactory factory, Type shadowType, string shadowName, List<(Type Type, Type TrackedEntity)> trackers)
    {
        this._factory = factory;
        this._shadowType = shadowType;
        this._shadowName = shadowName;
        this._trackers = trackers;
        CacheManagers();
    }

    private void CacheManagers()
    {
        foreach (var tracker in _trackers)
        {
            var list = _managers[tracker.TrackedEntity] = [];
            foreach (var field in tracker.Type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)) 
            {
                if (typeof(TrackerUpdateAction).IsAssignableFrom(field.FieldType))
                {
                    list.Add(CreateManagerForEntity(tracker, field, 
                        (TrackerUpdateAction)field.GetValue(null)!));
                }
            }
        }
    }

    private IBatchManagerForShadowAndEntity<TShadow> CreateManagerForEntity((Type Type, Type TrackedEntity) tracker, FieldInfo field, TrackerUpdateAction updateAction)
    {
        var m = this.GetType()
            .GetMethod("CreateManagerForEntityStrong", BindingFlags.Instance | BindingFlags.NonPublic)!;

        m = m.MakeGenericMethod(tracker.TrackedEntity);
        return (IBatchManagerForShadowAndEntity<TShadow>)m.Invoke(this, [tracker.Type, field, updateAction])!;
    }
    
    private IBatchManagerForShadowAndEntity<TShadow> CreateManagerForEntityStrong<TEntity>(Type trackerType, FieldInfo field, TrackerUpdateAction updateAction) where TEntity : class, IAggregateRoot
    {
        return new BatchManagerForShadowAndEntity<TShadow, TEntity>(_factory, trackerType, field, updateAction);
    }
    
    public IEnumerable<IBatchManagerForEvent> GetManagersForEntity(Type trackedEntityType, EntityChangedIE evt, object? originalShadow)
    {
        var shadow = evt.ShadowJson.FromJson<TShadow>()!;
        foreach (var manager in _managers[trackedEntityType])
        {
            if (manager.AppliesTo(shadow, originalShadow, evt.EventKey))
            {
                yield return manager.GetBatchManagerForShadowAndEntity(evt);
            }
        }
    }
}