
namespace Pulsar.BuildingBlocks.Sync.Service
{
    public interface ISyncIntegrationEventDispatcher
    {
        Task Run(CancellationToken ct);
    }
}