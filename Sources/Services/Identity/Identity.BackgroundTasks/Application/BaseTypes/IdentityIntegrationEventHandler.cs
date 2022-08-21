using MediatR;
using Pulsar.BuildingBlocks.EventBus.Abstractions;
using Pulsar.BuildingBlocks.EventBus.Contracts;
using Pulsar.BuildingBlocks.EventBus.Events;

namespace Pulsar.Services.Identity.BackgroundTasks.Application.BaseTypes;

public abstract class IdentityIntegrationEventHandler<TEvent> : IIntegrationEventHandler<TEvent> where TEvent : IntegrationEvent
{
    protected IMediator Mediator { get; }
    protected ILogger Logger { get; }

    public IdentityIntegrationEventHandler(ILogger<IdentityIntegrationEventHandler<TEvent>> logger, IMediator mediator)
    {
        Mediator = mediator;
        Logger = logger;
    }

    protected abstract Task Handle(TEvent evt);

    async Task IIntegrationEventHandler<TEvent>.Handle(TEvent @event)
    {
        var eventName = EventNameAttribute.GetEventName(typeof(TEvent));
        Logger.LogInformation("----- Handling integration event {EventName} ({@Event})", eventName, @event);
        try
        {
            await Handle(@event);
            Logger.LogInformation("----- Integration event {EventName} handled", eventName);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "----- Integration event {EventName} error", eventName);
            throw;
        }
    }
}
