using MongoDB.Bson;
using Pulsar.BuildingBlocks.Sync.Contracts;

namespace Pulsar.BuildingBlocks.Sync.Functions.Implementations;

public record PortableActivityDescription(string Key, EntityChangedIE? @Event, ObjectId? BatchId)
{
    public const string PREPARE_BATCHES = "PrepareBatches";
    public const string EXECUTE_BATCH = "ExecuteBatch";
}
