using DDD.Contracts;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Pulsar.BuildingBlocks.DDD;
using Pulsar.BuildingBlocks.Sync.Contracts;

namespace Pulsar.BuildingBlocks.Sync.Functions.Entities;

[method: BsonConstructor]
public class SyncBatch(
    ObjectId id,
    Guid eventId,
    DateTime eventTimestamp,
    List<ObjectId> entitiesToUpdate,
    string shadowJson,
    string shadowType,
    string shadowAssembly,
    string entityType,
    string entityAssembly,
    string trackerType,
    string trackerAssembly,
    string trackerRule,
    ChangedEventKey eventKey,
    ObjectId changedEntityId,
    DateTime changeTimestamp) : AggregateRoot(id)
{
    public Guid EventId { get; set; } = eventId;
    public DateTime EventTimestamp { get; set; } = eventTimestamp;
    public List<ObjectId> EntitiesToUpdate { get; set; } = entitiesToUpdate;
    public string ShadowJson { get; set; } = shadowJson;
    public string ShadowType { get; set; } = shadowType;
    public string ShadowAssembly { get; set; } = shadowAssembly;
    public string EntityType { get; set; } = entityType;
    public string EntityAssembly { get; set; } = entityAssembly;
    public string TrackerType { get; set; } = trackerType;
    public string TrackerAssembly { get; set; } = trackerAssembly;
    public string TrackerRule { get; set; } = trackerRule;
    public ChangedEventKey EventKey { get; private set; } = eventKey;
    public ObjectId ChangedEntityId { get; private set; } = changedEntityId;
    public DateTime ChangeTimestamp { get; private set; } = changeTimestamp;
}