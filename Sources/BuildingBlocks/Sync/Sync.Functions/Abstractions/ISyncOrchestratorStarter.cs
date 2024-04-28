using Microsoft.DurableTask.Client;
using Pulsar.BuildingBlocks.Sync.Contracts;

namespace Pulsar.BuildingBlocks.Sync.Functions.Abstractions;

public interface ISyncOrchestratorStarter<TFunctionClass> where TFunctionClass : class
{
    Task Start(EntityChangedIE evt, DurableTaskClient client);
}
