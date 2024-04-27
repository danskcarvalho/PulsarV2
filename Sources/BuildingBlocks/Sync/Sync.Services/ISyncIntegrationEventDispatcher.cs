
namespace Pulsar.BuildingBlocks.Sync.Services
{
    public interface ISyncIntegrationEventDispatcher
    {
        Task Run(CancellationToken ct);
    }
}