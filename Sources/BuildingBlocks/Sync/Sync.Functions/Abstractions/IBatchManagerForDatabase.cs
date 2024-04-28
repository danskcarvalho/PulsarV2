using MongoDB.Bson;

namespace Pulsar.BuildingBlocks.Sync.Functions.Abstractions;

public interface IBatchManagerForDatabase
{
    Task<IBatch> GetFromId(ObjectId batchId);
}