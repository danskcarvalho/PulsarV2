using Pulsar.BuildingBlocks.DDD.Abstractions;
using Pulsar.BuildingBlocks.Sync.Contracts;
using System.Reflection;
using MediatR;

namespace Pulsar.BuildingBlocks.Sync.Domain;

public abstract class Tracker
{

}

public class Tracker<TEntity> : Tracker where TEntity : class, IAggregateRoot
{
    protected static ForShadow<TShadow> For<TShadow>() where TShadow : class
    {
        return new ForShadow<TShadow>();
    }

    public class ForShadow<TShadow> where TShadow : class
    {
        private readonly List<Func<object, object?>> _onChanged = new List<Func<object, object?>>();
        private ChangedEventKey? _eventKey;
        private Func<TShadow?, INotification?>? _sendNotification;

        public ForShadow<TShadow> On<TValue>(Func<TShadow, TValue?> onChanged)
        {
            _eventKey = null;
            _onChanged.Add(obj => onChanged((TShadow)obj));
            return this;
        }

        public ForShadow<TShadow> On(ChangedEventKey key)
        {
            _onChanged.Clear();
            _eventKey = key;
            return this;
        }

        public ForShadow<TShadow> Send<TNotification>(Func<TShadow?, TNotification?> notificationFn) where TNotification : INotification
        {
            _sendNotification = s => notificationFn(s);
            return this;
        }

        public TrackerAction NoUpdate()
        {
            if (_sendNotification == null)
            {
                throw new InvalidOperationException("must set notification");
            }
            
            return new TrackerAction<TEntity>(typeof(TShadow), GetShadowName(), _onChanged, _eventKey,
                null,
                _sendNotification != null ? obj => _sendNotification((TShadow?)obj) : null);
        }

        public TrackerAction Update(Func<TShadow?, IUpdateSpecification<TEntity>> updateFunction)
        {
            return new TrackerAction<TEntity>(typeof(TShadow), GetShadowName(), _onChanged, _eventKey,
                obj => updateFunction((TShadow?)obj),
                _sendNotification != null ? obj => _sendNotification((TShadow?)obj) : null);
        }

        private string GetShadowName()
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
