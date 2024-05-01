using System.ComponentModel;
using MediatR;
using MongoDB.Bson;

namespace Pulsar.BuildingBlocks.DDD.Abstractions;

public interface IAggregateRoot : ISupportInitialize
{
    ObjectId Id { get; }
    long Version { get; }
    bool IsTransient { get; }

    IReadOnlyCollection<INotification> DomainEvents { get; }
    void AddDomainEvent(INotification eventItem);
    void RemoveDomainEvent(INotification eventItem);
    void ClearDomainEvents();
    void IncVersion();
}
