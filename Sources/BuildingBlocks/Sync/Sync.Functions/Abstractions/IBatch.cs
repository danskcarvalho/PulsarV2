using MongoDB.Bson;

namespace Pulsar.BuildingBlocks.Sync.Functions.Abstractions;

public interface IBatch
{
    ObjectId BatchId { get; }
    Task Execute();
}