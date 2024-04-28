using Microsoft.DurableTask;

namespace Pulsar.BuildingBlocks.Sync.Functions.Abstractions;

public interface ISyncActivity
{
    Task<string> Execute(string input);
}
