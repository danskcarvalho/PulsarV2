using Pulsar.BuildingBlocks.DDD.Abstractions;
using Pulsar.BuildingBlocks.Sync.Contracts;
using System.Linq.Expressions;
using System.Reflection;

namespace Pulsar.BuildingBlocks.Sync.Domain;

public abstract class Tracker
{

}

public class Tracker<TCollectionType> : Tracker
{
    protected static ForShadow<TShadow> For<TShadow>() where TShadow : class
    {
        return new ForShadow<TShadow>();
    }

    public class ForShadow<TShadow> where TShadow : class
    {
        private readonly List<Func<object, object?>> _OnChanged = new List<Func<object, object?>>();
        private ChangedEventKey? _EventKey = null;

        public ForShadow<TShadow> On<TValue>(Func<TShadow, TValue?> onChanged)
        {
            _EventKey = null;
            _OnChanged.Add(obj => onChanged((TShadow)obj));
            return this;
        }

        public ForShadow<TShadow> On(ChangedEventKey key)
        {
            _OnChanged.Clear();
            _EventKey = key;
            return this;
        }

        public TrackerUpdateAction Update(Func<TShadow, IUpdateSpecification<TCollectionType>> updateFunction)
        {
            return new TrackerUpdateAction<TCollectionType>(typeof(TShadow), GetEntityName(), _OnChanged, _EventKey, obj => updateFunction((TShadow)obj));
        }

        private string GetEntityName()
        {
            var attr = typeof(TShadow).GetCustomAttribute<ShadowAttribute>();
            if (attr == null)
            {
                throw new InvalidOperationException($"no ShadowAttribute on type {typeof(TShadow).FullName}");
            }
            return attr.Name;
        }
    }
}
