using MediatR;
using Pulsar.BuildingBlocks.DDD.Abstractions;
using Pulsar.BuildingBlocks.Sync.Contracts;

namespace Pulsar.BuildingBlocks.Sync.Domain;

public record TrackerAction(
    Type ShadowType, 
    string ShadowName, 
    IReadOnlyList<Func<object, object?>> OnChanged, 
    ChangedEventKey? EventKey,
    Func<object?, INotification?>? SendNotification)
{
}

public record TrackerAction<TCollectionType>(
    Type ShadowType, string ShadowName,
    IReadOnlyList<Func<object, object?>> OnChanged,
    ChangedEventKey? EventKey,
    Func<object?, IUpdateSpecification<TCollectionType>>? UpdateFunction,
    Func<object?, IDeleteSpecification<TCollectionType>>? DeleteFunction,
    Func<object?, TCollectionType[]>? InsertFunction,
    Func<object?, INotification?>? SendNotification) : TrackerAction(ShadowType, ShadowName, OnChanged, EventKey, SendNotification)
{
}