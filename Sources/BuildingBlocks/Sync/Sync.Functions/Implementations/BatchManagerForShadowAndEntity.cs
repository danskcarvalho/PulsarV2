using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using MongoDB.Bson;
using Pulsar.BuildingBlocks.DDD.Abstractions;
using Pulsar.BuildingBlocks.Sync.Contracts;
using Pulsar.BuildingBlocks.Sync.Domain;
using Pulsar.BuildingBlocks.Sync.Functions.Abstractions;
using Pulsar.BuildingBlocks.Sync.Functions.Entities;
using Pulsar.BuildingBlocks.Utils;

namespace Pulsar.BuildingBlocks.Sync.Functions.Implementations;

public class BatchManagerForShadowAndEntity<TShadow, TEntity>(
    Type tracker,
    FieldInfo rule,
    TrackerAction updateAction)
    : IBatchManagerForShadowAndEntity<TShadow, TEntity>
    where TShadow : class, IShadow
    where TEntity : class, IAggregateRoot
{
    private const int PARTITION_SIZE = 10_000;

    private readonly TrackerAction<TEntity> _updateAction = (TrackerAction<TEntity>)updateAction;
    private EntityChangedIE? _entityChanged;
    private object? _shadow;

    public bool AppliesTo(object? currentShadow, object? previousShadow, ChangedEventKey eventKey)
    {
        if (_updateAction.EventKey == eventKey)
        {
            return true;
        }
        else
        {
            return AppliesTo(_updateAction, currentShadow, previousShadow);
        }
            
    }

    private bool AppliesTo(TrackerAction<TEntity> updateAction, object? currentShadow, object? previousShadow)
    {
        if (previousShadow == null || currentShadow == null)
        {
            return updateAction.SendNotification != null;
        }

        foreach (var fnChanged in updateAction.OnChanged)
        {
            var currentValue = fnChanged(currentShadow);
            var previousValue = fnChanged(previousShadow);

            if (!CompareValues(currentValue, previousValue))
            {
                return true;
            }
        }

        return false;
    }

    private bool CompareValues(object? currentValue, object? previousValue)
    {
        if (ReferenceEquals(currentValue, previousValue))
        {
            return true;
        }

        if (currentValue == null || previousValue == null)
        {
            return false;
        }

        if (currentValue is IDictionary d1 && previousValue is IDictionary d2)
        {
            if (d1.Count != d2.Count)
            {
                return false;
            }

            foreach (var key in d1.Keys)
            {
                if (!d2.Contains(key))
                {
                    return false;
                }

                if (!CompareValues(d1[key], d2[key]))
                {
                    return false;
                }
            }

            return true;
        }
        else if (currentValue is IList l1 && previousValue is IList l2)
        {
            if (l1.Count != l2.Count)
            {
                return false;
            }

            for (int i = 0; i < l1.Count; i++)
            {
                if (!CompareValues(l1[i], l2[i]))
                {
                    return false;
                }
            }

            return true;
        }
        else if (IsHashSet(currentValue, previousValue))
        {
            var m = currentValue.GetType().GetMethod("SetEquals");
            return (bool)m!.Invoke(currentValue, [previousValue])!;
        }
        else
        {
            return Equals(currentValue, previousValue);
        }
    }

    private bool IsHashSet(object currentValue, object previousValue)
    {
        var t1 = currentValue.GetType();
        var t2 = previousValue.GetType();

        if (!t1.IsConstructedGenericType || t1.GetGenericTypeDefinition() != typeof(HashSet<>))
        {
            return false;
        }

        var element1 = t1.GetGenericArguments()[0];
        
        if (!t2.IsConstructedGenericType || t2.GetGenericTypeDefinition() != typeof(HashSet<>))
        {
            return false;
        }

        var element2 = t2.GetGenericArguments()[0];

        return element1 == element2;
    }

    public IBatchManagerForShadowAndEntity<TShadow> FromEvent(EntityChangedIE evt)
    {
        return new BatchManagerForShadowAndEntity<TShadow, TEntity>(tracker, rule, _updateAction)
        {
            _entityChanged = evt,
            _shadow = evt.ShadowJson?.FromJsonString<TShadow>()
        };
    }

    public async Task<List<IBatch>> GetBatches(ISyncDbContextFactory factory)
    {
        if (_shadow == null || _entityChanged == null)
        {
            throw new InvalidOperationException("must call GetBatchManagerForShadowAndEntity first");
        }

        return await factory.Execute<TEntity, List<IBatch>>(async ctx =>
        {
            if (_updateAction.UpdateFunction != null)
            {
                return await CreateBatchesForUpdatingDatabase(factory, ctx);
            }
            else
            {
                var batch = new SyncBatch(
                    ObjectId.GenerateNewId(),
                    _entityChanged.Id,
                    _entityChanged.CreationDate,
                    [],
                    _entityChanged.ShadowJson,
                    typeof(TShadow).FullName!,
                    typeof(TShadow).Assembly.FullName!,
                    typeof(TEntity).FullName!,
                    typeof(TEntity).Assembly.FullName!,
                    tracker.FullName!,
                    tracker.Assembly.FullName!,
                    rule.Name,
                    _entityChanged.EventKey,
                    _entityChanged.ChangedEntityId,
                    _entityChanged.ChangeTimestamp
                );
                
                await ctx.SyncBatchRepository.InsertOneAsync(batch);
                return [new Batch<TShadow, TEntity>(factory, batch.Id)];
            }
        });
    }

    private async Task<List<IBatch>> CreateBatchesForUpdatingDatabase(ISyncDbContextFactory factory, SyncDbContext<TEntity> ctx)
    {
        var spec = _updateAction.UpdateFunction!(_shadow!).GetSpec();
        var idsSpec = new FindEntitiesSpecification(spec.Predicate);
        var allIds = await ctx.EntityRepository.FindManyAsync(idsSpec);
        var batchesToInsert = new List<SyncBatch>();

        while (allIds.Count > 0)
        {
            var partitioned = allIds.Select(x => x.Id).Partition(PARTITION_SIZE).ToList();

            foreach (var ids in partitioned)
            {
                var batch = new SyncBatch(
                    ObjectId.GenerateNewId(),
                    _entityChanged!.Id,
                    _entityChanged.CreationDate,
                    ids,
                    _entityChanged.ShadowJson,
                    typeof(TShadow).FullName!,
                    typeof(TShadow).Assembly.FullName!,
                    typeof(TEntity).FullName!,
                    typeof(TEntity).Assembly.FullName!,
                    tracker.FullName!,
                    tracker.Assembly.FullName!,
                    rule.Name,
                    _entityChanged.EventKey,
                    _entityChanged.ChangedEntityId,
                    _entityChanged.ChangeTimestamp
                );

                batchesToInsert.Add(batch);
            }


            idsSpec = new FindEntitiesSpecification(spec.Predicate, allIds[^1].Id);
            allIds = await ctx.EntityRepository.FindManyAsync(idsSpec);
        }

        await ctx.SyncBatchRepository.InsertManyAsync(batchesToInsert);

        return batchesToInsert.Select(x => (IBatch)new Batch<TShadow, TEntity>(factory, x.Id)).ToList();
    }

    class OnlyId(ObjectId id)
    {
        public ObjectId Id { get; set; } = id;
    }

    class FindEntitiesSpecification : IFindSpecification<TEntity, OnlyId>
    {
        private const int MAX_ENTITIES = 1_000_000;
        
        public FindEntitiesSpecification(Expression<Func<TEntity, bool>> predicate, ObjectId? lastId = null)
        {
            Predicate = predicate;
            LastId = lastId;
        }

        public Expression<Func<TEntity, bool>> Predicate { get; }
        public ObjectId? LastId { get; }
        public FindSpecification<TEntity, OnlyId> GetSpec()
        {
            if (LastId == null)
            {
                return Find.Where(Predicate)
                    .OrderBy(x => x.Id)
                    .Limit(MAX_ENTITIES)
                    .Select<OnlyId>(x => new OnlyId(x.Id)).Build();
            }
            else
            {
                return Find.Where(Predicate.AndAlso(x => x.Id > LastId))
                    .OrderBy(x => x.Id)
                    .Limit(MAX_ENTITIES)
                    .Select<OnlyId>(x => new OnlyId(x.Id)).Build();
            }
        }
    }
}