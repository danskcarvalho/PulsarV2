

using Pulsar.Services.PushNotification.API.Application.BaseTypes;

namespace Pulsar.Services.PushNotification.API.Application.Queries;

public class NotificacoesPushQueries : PushNotificationQueries, INotificacoesPushQueries
{
    public NotificacoesPushQueries(PushNotificationQueriesContext ctx) : base(ctx)
    {
    }
}
