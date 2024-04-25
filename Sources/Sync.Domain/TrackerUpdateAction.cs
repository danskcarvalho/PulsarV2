using Pulsar.BuildingBlocks.DDD.Abstractions;
using Pulsar.BuildingBlocks.Sync.Contracts;

namespace Pulsar.BuildingBlocks.Sync.Domain;

public record TrackerUpdateAction(Type ShadowType, string EntityName, IReadOnlyList<Func<object, object?>> OnChanged, ChangedEventKey? EventKey)
{
}

public record TrackerUpdateAction<TCollectionType>(
    Type ShadowType, string EntityName,
    IReadOnlyList<Func<object, object?>> OnChanged,
    ChangedEventKey? EventKey,
    Func<object, IUpdateSpecification<TCollectionType>> UpdateFunction) : TrackerUpdateAction(ShadowType, EntityName, OnChanged, EventKey)
{
}