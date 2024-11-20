using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FluentUI.AspNetCore.Components;
using Pulsar.Web.Client.Clients.PushNotification;
using System.Reflection;

namespace Pulsar.Web.Client.Services.PushNotifications;

public static class DIExtensions
{
	public static IServiceCollection AddPushNotificationServices(this IServiceCollection services, Action<PushNotificationServiceOptions> setOptions)
	{
		var options = new PushNotificationServiceOptions();
		setOptions(options);

		services.AddSingleton<PushNotificationService>(sp =>
		{
			return new PushNotificationService(
				sp.GetRequiredService<IPushNotificationClient>(),
				sp.GetRequiredService<ILogger<PushNotificationService>>(),
				sp.GetRequiredService<IMediator>(),
				options.AssembliesToScanForIntegrationEvents);
		});
		services.AddSingleton<PushNotificationManager>(sp =>
		{
			return new PushNotificationManager(
				sp.GetRequiredService<PushNotificationService>(),
				sp.GetRequiredService<IPushNotificationClient>(),
				sp.GetRequiredService<IMediator>(),
				sp.GetRequiredService<NavigationManager>(),
				options.AssembliesToScanForRoutingActions,
				sp.GetRequiredService<ILogger<PushNotificationManager>>()
				);
		});

		return services;
	}
}

public class PushNotificationServiceOptions
{
	private List<Assembly> _assembliesToScanForIntegrationEvents = new();
	private List<Assembly> _assembliesToScanForRoutingActions = new();

	public List<Assembly> AssembliesToScanForIntegrationEvents => _assembliesToScanForIntegrationEvents;
	public List<Assembly> AssembliesToScanForRoutingActions => _assembliesToScanForRoutingActions;

	public void AddAssembliesToScanForIntegrationEvents(params Assembly[] assembliesToScanForIntegrationEvents)
	{
		_assembliesToScanForIntegrationEvents.AddRange(assembliesToScanForIntegrationEvents);
	}

	public void AddAssembliesToScanForRoutingActions(params Assembly[] assembliesToScanForRoutingActions)
	{
		_assembliesToScanForIntegrationEvents.AddRange(assembliesToScanForRoutingActions);
	}
}
