

using Pulsar.Services.PushNotification.API.Application.BaseTypes;

namespace Pulsar.Services.PushNotification.API.Application.Queries;

public class PushNotificationsQueries : PushNotificationQueries, IPushNotificationQueries
{
    public PushNotificationsQueries(PushNotificationQueriesContext ctx) : base(ctx)
    {
    }
}
