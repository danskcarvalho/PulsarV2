using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Pulsar.BuildingBlocks.EventBus.Contracts;
using System.Text.Json.Serialization;
using Pulsar.BuildingBlocks.EventBus.Events;

namespace Pulsar.BuildingBlocks.Sync.Contracts;

[EventName("General:EntityChanged")]
public record EntityChangedIE : IntegrationEvent
{
    public required string ShadowName { get; init; }
    public required ChangedEventKey EventKey { get; init; }
    public required ObjectId ChangedEntityId { get; init; }
    public required string? ShadowJson { get; init; }
    public required DateTime ChangeTimestamp { get; init; }
}
