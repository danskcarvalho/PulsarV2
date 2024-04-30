using MediatR;
using Pulsar.BuildingBlocks.DDD.Abstractions;
using Pulsar.BuildingBlocks.Sync.Contracts;

namespace Pulsar.BuildingBlocks.Sync.Domain;

public record TrackerUpdateAction(
    Type ShadowType, 
    string ShadowName, 
    IReadOnlyList<Func<object, object?>> OnChanged, 
    ChangedEventKey? EventKey,
    Func<object, INotification>? SendNotification)
{
}

public record TrackerUpdateAction<TCollectionType>(
    Type ShadowType, string ShadowName,
    IReadOnlyList<Func<object, object?>> OnChanged,
    ChangedEventKey? EventKey,
    Func<object, IUpdateSpecification<TCollectionType>>? UpdateFunction,
    Func<object, INotification>? SendNotification) : TrackerUpdateAction(ShadowType, ShadowName, OnChanged, EventKey, SendNotification)
{
}