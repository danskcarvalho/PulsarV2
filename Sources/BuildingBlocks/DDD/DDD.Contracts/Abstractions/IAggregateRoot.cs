using System.ComponentModel;
using DDD.Contracts;
using MediatR;
using MongoDB.Bson;

namespace Pulsar.BuildingBlocks.DDD.Abstractions;

public interface IAggregateRoot : ISupportInitialize
{
    ObjectId Id { get; }
    long Version { get; }
    long? LastVersion { get; set; }
    bool IsTransient { get; }
    SyncPendingKey? SyncPendingKey { get; set; }
    bool IsSyncPending { get; set; }

    IReadOnlyCollection<INotification> DomainEvents { get; }
    void AddDomainEvent(INotification eventItem);
    void RemoveDomainEvent(INotification eventItem);
    void ClearDomainEvents();
    void IncVersion(bool force = false);
}
