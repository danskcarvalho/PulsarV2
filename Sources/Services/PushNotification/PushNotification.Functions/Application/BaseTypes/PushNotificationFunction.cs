namespace Pulsar.Services.PushNotification.Functions.Application.BaseTypes;

public class PushNotificationFunction(IMediator mediator)
{
    protected IMediator Mediator => mediator;
}
