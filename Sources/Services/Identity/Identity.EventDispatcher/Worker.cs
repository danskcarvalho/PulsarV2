using Pulsar.BuildingBlocks.EventBus.Abstractions;
using Pulsar.BuildingBlocks.Sync.Services;

namespace Pulsar.Services.Identity.EventDispatcher;

public class Worker : BackgroundService
{
    private readonly IIntegrationEventDispatcherService _eventService;
    private readonly ISyncIntegrationEventDispatcher _syncService;

    public Worker(IIntegrationEventDispatcherService service, ISyncIntegrationEventDispatcher syncService)
    {
        _eventService = service;
        _syncService = syncService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var t1 = _eventService.Run(stoppingToken);
        var t2 = _syncService.Run(stoppingToken);
        await Task.WhenAll(t1, t2);
    }
}