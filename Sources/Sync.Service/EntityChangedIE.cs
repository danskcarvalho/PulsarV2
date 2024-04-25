using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Pulsar.BuildingBlocks.EventBus.Contracts;
using Pulsar.BuildingBlocks.EventBus.Events;
using Pulsar.BuildingBlocks.Sync.Contracts;
using System.Text.Json.Serialization;

namespace Pulsar.BuildingBlocks.Sync.Service;

[EventName("General:EntityChanged")]
public record EntityChangedIE : IntegrationEvent
{
    public string EntityName { get; }
    public ChangedEventKey EventKey { get; }
    public ObjectId EntityId { get; }
    public string Json { get; }
    public DateTime ChangeTimestamp { get; }

    [BsonConstructor, JsonConstructor]
    public EntityChangedIE(string entityName, ChangedEventKey eventKey, ObjectId entityId, string json, DateTime changeTimestamp)
    {
        EntityName = entityName;
        EntityId = entityId;
        Json = json;
        ChangeTimestamp = changeTimestamp;
        EventKey = eventKey;
    }
}
