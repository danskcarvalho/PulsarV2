using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Microsoft.FluentUI.AspNetCore.Components;
using Pulsar.BuildingBlocks.EventBus.Contracts.PushNotifications;
using Pulsar.Services.PushNotification.Contracts.Commands.PushNotifications;
using Pulsar.Services.PushNotification.Contracts.DTOs;
using Pulsar.Services.Shared.PushNotifications;
using Pulsar.Web.Client.Clients.PushNotification;
using System.Diagnostics.Metrics;
using System.Reflection;

namespace Pulsar.Web.Client.Services.PushNotifications;

public partial class PushNotificationManager(
	PushNotificationService service,
	IPushNotificationClient pushNotificationClient,
	IMessageService messageService,
	IToastService toastService,
	IMediator mediator,
	NavigationManager navigationManager,
	List<Assembly> assembliesToScan,
	ILogger<PushNotificationManager> logger) : IAsyncDisposable
{
	private bool _started = false;
	private bool _scanned = false;
	public const int MAX_NOTIFICATIONS = 200;
	public const string NOTIFICATION_CENTER_SECTION_UNREAD = "NOTIFICATION_CENTER_SECTION_UNREAD";
	public const string NOTIFICATION_CENTER_SECTION_HISTORY = "NOTIFICATION_CENTER_SECTION_HISTORY";
	public static readonly int TOAST_TIMEOUT = 8000; // 8s


	private readonly Dictionary<PushNotificationRouteKey, List<EventHandler<PushNotificationActionEvent>>> _actionHandlers = new();
	private List<PushNotificationDataWithId> _notifications = new List<PushNotificationDataWithId>();
	private Dictionary<PushNotificationRouteKey, PushNotificationRoutingAttribute> _routeHandlers = new();
	private int _unread = 0;

	public event EventHandler? OnNotificationListChanged;

	public Action Subscribe(PushNotificationRouteKey routeKey, EventHandler<PushNotificationActionEvent> eventHandler)
	{
		if (!_actionHandlers.ContainsKey(routeKey))
		{
			_actionHandlers[routeKey] = new();
		}

		_actionHandlers[routeKey].Add(eventHandler);

		return () => _actionHandlers[routeKey].Remove(eventHandler);
	}

	private void ScanAssemblies()
	{
		if (_scanned)
		{
			return;
		}

		_scanned = true;
		foreach (var assembly in assembliesToScan)
		{
			var types = assembly.GetTypes().Where(t => t.GetCustomAttribute<PushNotificationRoutingAttribute>() != null);
			foreach (var type in types)
			{
				var attr = type.GetCustomAttribute<PushNotificationRoutingAttribute>()!;
				
				_routeHandlers[attr.RouteKey] = attr;
			}
		}
	}

	public async Task Start()
	{
		try
		{
			if (_started)
			{
				return;
			}

			ScanAssemblies();
			var t1 = service.Start();
			var t2 = pushNotificationClient.Get(excluirLidas: false);

			await t1;
			_notifications.Clear();
			_notifications.AddRange(await t2);
			_notifications = _notifications.OrderByDescending(n => n.CreatedOn).ToList();
			_unread = _notifications.Count(n => !n.IsRead);
			service.OnPushNotification += Service_OnPushNotification;
			OnNotificationListChanged?.Invoke(this, EventArgs.Empty);

			messageService.Clear(NOTIFICATION_CENTER_SECTION_UNREAD);
			messageService.Clear(NOTIFICATION_CENTER_SECTION_HISTORY);

			foreach (var notification in ((IEnumerable<PushNotificationDataWithId>)_notifications).Reverse())
			{
				ShowPushNotification(notification, noToast: true);
			}
			_started = true;
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "error when starting push notification manager");
		}
	}

	private void Service_OnPushNotification(object? sender, PushNotificationEvent e)
	{
		_notifications.Insert(0, e.PushNotificationData);
		if (_notifications.Count > MAX_NOTIFICATIONS)
		{
			_notifications = _notifications.Take(MAX_NOTIFICATIONS).ToList();
		}
		_unread = _notifications.Count(n => !n.IsRead);
		OnNotificationListChanged?.Invoke(this, EventArgs.Empty);
		ShowPushNotification(e.PushNotificationData);
	}

	public IReadOnlyList<PushNotificationDataWithId> Notifications => _notifications.AsReadOnly();
	public int Unread => _unread;

	public async Task MarkAllRead()
	{
		if (_notifications.Count == 0)
		{
			return;
		}
		List<string> mark = new List<string>();
		foreach (var notification in _notifications)
		{
			if (!notification.IsRead)
			{
				mark.Add(notification.PushNotificationId);
			}
			notification.IsRead = true;
		}
		_unread = 0;
		OnNotificationListChanged?.Invoke(this, EventArgs.Empty);
		if (mark.Count > 0)
		{
			await pushNotificationClient.MarcarComoLida(new MarcarNotificacoesComoLidaCmd(mark));
		}
	}

	public async Task MarkAsRead(string pushNotificationId)
	{
		if (_notifications.Count == 0)
		{
			return;
		}

		List<string> mark = new List<string>();
		foreach (var notification in _notifications)
		{
			if (notification.PushNotificationId == pushNotificationId)
			{
				mark.Add(notification.PushNotificationId);
				notification.IsRead = true;
				break;
			}
		}
		_unread = _notifications.Count(n => !n.IsRead);
		OnNotificationListChanged?.Invoke(this, EventArgs.Empty);
		if (mark.Count > 0)
		{
			await pushNotificationClient.MarcarComoLida(new MarcarNotificacoesComoLidaCmd(mark));
		}
	}

	private void ShowPushNotification(PushNotificationDataWithId notificacao, bool noToast = false)
	{
		if (notificacao.Display == PushNotificationDisplay.All || notificacao.Display == PushNotificationDisplay.Toast)
		{
			if (!noToast)
			{
				ShowToast(notificacao);
			}
		}
		if (notificacao.Display == PushNotificationDisplay.All || notificacao.Display == PushNotificationDisplay.NotificationCenter)
		{
			ShowNotificationCenter(notificacao);
		}
	}

	public async ValueTask DisposeAsync()
	{
		service.OnPushNotification -= Service_OnPushNotification;
		await service.DisposeAsync();
	}
}
