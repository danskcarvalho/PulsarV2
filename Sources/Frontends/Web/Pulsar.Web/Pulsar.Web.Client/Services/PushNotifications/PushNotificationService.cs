using MediatR;
using Microsoft.AspNetCore.SignalR.Client;
using Pulsar.BuildingBlocks.EventBus.Contracts.PushNotifications;
using Pulsar.Services.Shared;
using Pulsar.Services.Shared.PushNotifications;
using Pulsar.Web.Client.Clients.PushNotification;
using System.Diagnostics;
using System.Reflection;

namespace Pulsar.Web.Client.Services.PushNotifications;

public class PushNotificationService(
	IPushNotificationClient pushNotificationClient,
	ILogger<PushNotificationService> logger,
	IMediator mediator,
	List<Assembly> assembliesToScan) : IAsyncDisposable
{
	private bool _started = false;
	private bool _scanned = false;
	private Dictionary<PushNotificationKey, List<Type>> _eventsToFire = new ();
	private Dictionary<PushNotificationKey, List<EventHandler<PushNotificationEvent>>> _eventHandlersForKey = new();
	private Dictionary<Type, List<Delegate>> _eventHandlersForData = new();
	private HubConnection? _connection = null;

	public event EventHandler<PushNotificationEvent>? OnPushNotification;

	public Action Subscribe(PushNotificationKey key, EventHandler<PushNotificationEvent> eventHandler)
	{
		if(!_eventHandlersForKey.ContainsKey(key) )
		{
			_eventHandlersForKey[key] = new ();
		}

		_eventHandlersForKey[key].Add(eventHandler);

		return () => _eventHandlersForKey[key].Remove(eventHandler);
	}

	public Action Subscribe<TData>(EventHandler<PushNotificationEvent<TData>> eventHandler) where TData : class
	{
		if (!_eventHandlersForData.ContainsKey(typeof(TData)))
		{
			_eventHandlersForData[typeof(TData)] = new();
		}

		_eventHandlersForData[typeof(TData)].Add(eventHandler);

		return () => _eventHandlersForData[typeof(TData)].Remove(eventHandler);
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
			var types = assembly.GetTypes().Where(t => t.GetCustomAttribute<PushNotificationEventAttribute>() != null);
			foreach (var type in types)
			{
				var attr = type.GetCustomAttribute<PushNotificationEventAttribute>()!;
				if (!_eventsToFire.ContainsKey(attr.Key))
				{
					_eventsToFire[attr.Key] = new List<Type>();
				}
				_eventsToFire[attr.Key].Add(type);
			}
		}
	}

	public async Task Start()
	{
		ScanAssemblies();
		if (_started)
		{
			return;
		}
		try
		{
			var session = await pushNotificationClient.StartSession();
			_connection = new HubConnectionBuilder()
				.WithUrl(session.Url, options =>
				{
					options.Headers.Add("Authorization", $"Bearer {session.Token}");
				})
				.WithAutomaticReconnect()
				.Build();

			await _connection.StartAsync();
			Console.WriteLine($"Connection started: {session.Url}, {session.Token}");
			_started = true;

			_connection.On("published", (string pn) =>
			{
				OnPublish(mediator, pn.FromJsonString<PushNotificationDataWithId>());
			});
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "error trying to estabilish a connection to SignalR push notification service");
		}
	}

	private void OnPublish(IMediator mediator, PushNotificationDataWithId? pn)
	{
		if (pn == null)
		{
			return;
		}

		mediator.Publish(new PushNotificationEvent(pn));
		FireAdditionalEvents(pn);

		// Fire additional events
		OnPushNotification?.Invoke(this, new PushNotificationEvent(pn));
		if (_eventHandlersForKey.ContainsKey(pn.Key))
		{
			foreach (var handler in _eventHandlersForKey[pn.Key].ToList())
			{
				handler(this, new PushNotificationEvent(pn));
			}
		}
	}

	private void FireAdditionalEvents(PushNotificationDataWithId pn)
	{
		if (_eventsToFire.ContainsKey(pn.Key))
		{
			var list = _eventsToFire[pn.Key];
			if (list.Count > 0)
			{
				foreach (var type in list)
				{
					var notification = PushNotificationEvent.StronglyTyped(pn, type);
					mediator.Publish(notification);

					if (_eventHandlersForData.ContainsKey(type))
					{
						var strongEvent = PushNotificationEvent.StronglyTyped(pn, type);
						foreach (var handler in _eventHandlersForData[type].ToList())
						{
							handler.DynamicInvoke(this, strongEvent);
						}
					}
				}
			}
		}
	}

	public async ValueTask DisposeAsync()
	{
		if (_connection != null)
		{
			await _connection.StopAsync();
			await _connection.DisposeAsync();
			_connection = null;
		}
	}
}
