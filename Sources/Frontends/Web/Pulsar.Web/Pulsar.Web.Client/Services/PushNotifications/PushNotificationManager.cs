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
	SignalRManager signalRManager,
	IPushNotificationClient pushNotificationClient,
	IMediator mediator,
	NavigationManager navigationManager,
	List<Assembly> assembliesToScan,
	ILogger<PushNotificationManager> logger) : IAsyncDisposable
{
	private bool _started = false;
	private bool _scanned = false;
	private bool _inititalized = false;
	public const int MAX_NOTIFICATIONS = 200;
	public const string NOTIFICATION_CENTER_SECTION_UNREAD = "NOTIFICATION_CENTER_SECTION_UNREAD";
	public const string NOTIFICATION_CENTER_SECTION_HISTORY = "NOTIFICATION_CENTER_SECTION_HISTORY";
	public const string MESSAGES_TOP = "MESSAGES_TOP";
	public static readonly int TOAST_TIMEOUT = 8000; // 8s
	private List<Message> _messages = new List<Message>();


	private readonly Dictionary<PushNotificationRouteKey, List<EventHandler<PushNotificationActionEvent>>> _actionHandlers = new();
	private List<PushNotificationDataWithId> _notifications = new List<PushNotificationDataWithId>();
	private Dictionary<PushNotificationRouteKey, PushNotificationRoutingAttribute> _routeHandlers = new();
	private int _unread = 0;

	public IMessageService? MessageService { get; set; }
	public IToastService? ToastService { get; set; }
	public IReadOnlyList<PushNotificationDataWithId> Notifications => _notifications.AsReadOnly();
	public int Unread => _unread;

	public event EventHandler? NotificationListChanged;

	public event EventHandler<PushNotificationEvent>? PushNotificationReceived
	{
		add
		{
			signalRManager.PushNotificationReceived += value;
		}
		remove
		{
			signalRManager.PushNotificationReceived -= value;
		}
	}

	public Action Subscribe(PushNotificationRouteKey routeKey, EventHandler<PushNotificationActionEvent> eventHandler)
	{
		if (!_actionHandlers.ContainsKey(routeKey))
		{
			_actionHandlers[routeKey] = new();
		}

		_actionHandlers[routeKey].Add(eventHandler);

		return () => _actionHandlers[routeKey].Remove(eventHandler);
	}
	public Action Subscribe(PushNotificationKey key, EventHandler<PushNotificationEvent> eventHandler) => signalRManager.Subscribe(key, eventHandler);
	public Action Subscribe<TData>(EventHandler<PushNotificationEvent<TData>> eventHandler) where TData : class => signalRManager.Subscribe(eventHandler);
	public async Task Start()
	{
		try
		{
			if (_started)
			{
				return;
			}

			Initialize();
			var t1 = signalRManager.Start();
			await RefreshNotificationCenter();

			await t1;
			_started = true;
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "error when starting push notification manager");
		}
	}

	public async Task<bool> Reconnect()
	{
		try
		{	
			var t1 = signalRManager.Reconnect();
			await RefreshNotificationCenter();
			return await t1;
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "error when starting push notification manager");
			return false;
		}
	}
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
		MessageService?.Clear(NOTIFICATION_CENTER_SECTION_UNREAD);
		NotificationListChanged?.Invoke(this, EventArgs.Empty);
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
		NotificationListChanged?.Invoke(this, EventArgs.Empty);
		if (mark.Count > 0)
		{
			await pushNotificationClient.MarcarComoLida(new MarcarNotificacoesComoLidaCmd(mark));
		}
	}

	private async Task RefreshNotificationCenter()
	{
		var t2 = pushNotificationClient.Get(excluirLidas: false);

		_notifications.Clear();
		_notifications.AddRange(await t2);
		_notifications = _notifications.OrderByDescending(n => n.CreatedOn).ToList();
		_unread = _notifications.Count(n => !n.IsRead);
		NotificationListChanged?.Invoke(this, EventArgs.Empty);

		MessageService?.Clear(NOTIFICATION_CENTER_SECTION_UNREAD);
		MessageService?.Clear(NOTIFICATION_CENTER_SECTION_HISTORY);

		foreach (var notification in ((IEnumerable<PushNotificationDataWithId>)_notifications).Reverse())
		{
			ShowPushNotification(notification, noToast: true);
		}
	}
	private void Initialize()
	{
		if (_inititalized)
		{
			return;
		}

		ScanAssemblies();
		signalRManager.PushNotificationReceived += Service_PushNotificationReceived;
		signalRManager.Reconnecting += SignalRManager_Reconnecting;
		signalRManager.Reconnected += SignalRManager_Reconnected;
		signalRManager.Closed += SignalRManager_Closed;
		_inititalized = true;
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
	private void SignalRManager_Closed(object? sender, EventArgs e)
	{
		ShowClosedMessage();
	}
	private void ShowClosedMessage()
	{
		if (MessageService == null)
		{
			return;
		}
		ClearMessages();

		var message = "Conexão com o servidor perdida.";
		_messages.Add(MessageService.ShowMessageBar(options =>
		{
			options.Intent = MessageIntent.Error;
			options.Body = message;
			options.Title = "Notificações";
			options.Section = MESSAGES_TOP;
			options.AllowDismiss = true;
			options.Link = new ActionLink<Message>
			{
				Text = "Reconectar",
				Href = "javascript:void(0);",
				OnClick = async m =>
				{
					ShowReconnectingMessage();
					var connected = await this.Reconnect();
					if (!connected)
					{
						ShowClosedMessage();
					}
				}
			};
		}));
	}
	private void ClearMessages()
	{
		if (MessageService == null)
		{
			return;
		}

		foreach (var msg in _messages)
		{
			if (MessageService.AllMessages.Contains(msg))
			{
				MessageService.Remove(msg);
			}
		}
		_messages.Clear();
	}
	private async void SignalRManager_Reconnected(object? sender, EventArgs e)
	{
		if (MessageService == null)
		{
			return;
		}
		ClearMessages();

		var message = "Conexão com o servidor reestabelecida.";
		_messages.Add(MessageService.ShowMessageBar(options =>
		{
			options.Intent = MessageIntent.Success;
			options.Body = message;
			options.Title = "Notificações";
			options.Section = MESSAGES_TOP;
			options.Timeout = 10_000; //10 s
			options.AllowDismiss = true;
		}));
		await this.RefreshNotificationCenter();
	}
	private void SignalRManager_Reconnecting(object? sender, EventArgs e)
	{
		ShowReconnectingMessage();
	}
	private void ShowReconnectingMessage()
	{
		try
		{
			if (MessageService == null)
			{
				return;
			}
			ClearMessages();

			var message = "Reconectando-se com o servidor...";
			_messages.Add(MessageService.ShowMessageBar(options =>
			{
				options.Intent = MessageIntent.Warning;
				options.Body = message;
				options.Title = "Notificações";
				options.Section = MESSAGES_TOP;
				options.AllowDismiss = true;
			}));
		}
		catch { }
	}
	private void Service_PushNotificationReceived(object? sender, PushNotificationEvent e)
	{
		_notifications.Insert(0, e.PushNotificationData);
		if (_notifications.Count > MAX_NOTIFICATIONS)
		{
			_notifications = _notifications.Take(MAX_NOTIFICATIONS).ToList();
		}
		_unread = _notifications.Count(n => !n.IsRead);
		NotificationListChanged?.Invoke(this, EventArgs.Empty);
		ShowPushNotification(e.PushNotificationData);
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
		signalRManager.PushNotificationReceived -= Service_PushNotificationReceived;
		await signalRManager.DisposeAsync();
	}
}
