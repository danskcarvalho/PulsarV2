namespace Pulsar.BuildingBlocks.Sync.Functions.Abstractions;

public interface IBatchManagerForEvent
{
    Task<List<IBatch>> GetBatches();
}