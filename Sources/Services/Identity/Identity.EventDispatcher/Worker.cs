using Pulsar.BuildingBlocks.EventBus.Abstractions;

namespace Pulsar.Services.Identity.EventDispatcher;

public class Worker : BackgroundService
{
    private readonly IIntegrationEventDispatcherService _service;

    public Worker(IIntegrationEventDispatcherService service)
    {
        _service = service;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _service.Run(stoppingToken);
    }
}