using Microsoft.DurableTask;

namespace Pulsar.BuildingBlocks.Sync.Functions.Abstractions
{
    public interface ISyncOrchestrator<TFunctionClass> where TFunctionClass : class
    {
        Task Execute(TaskOrchestrationContext context);
    }
}
