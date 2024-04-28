using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace Pulsar.BuildingBlocks.Sync.Functions.Abstractions;

public interface ISyncActivity
{
    Task<string> Execute(IDurableActivityContext context);
}
