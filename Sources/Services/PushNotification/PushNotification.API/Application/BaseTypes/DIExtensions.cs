namespace Pulsar.Services.PushNotification.API.Application.BaseTypes;

public static class DIExtensions
{
	public static void AddQueries(this IServiceCollection collection)
	{
		collection.AddTransient<INotificacaoPushQueries, NotificacaoPushQueries>();
	}
}
