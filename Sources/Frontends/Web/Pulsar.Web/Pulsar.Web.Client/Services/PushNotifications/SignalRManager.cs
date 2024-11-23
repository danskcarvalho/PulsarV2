using MediatR;
using Microsoft.AspNetCore.SignalR.Client;
using Pulsar.BuildingBlocks.EventBus.Contracts.PushNotifications;
using Pulsar.Services.Shared;
using Pulsar.Services.Shared.PushNotifications;
using Pulsar.Web.Client.Clients.PushNotification;
using System.Diagnostics;
using System.Reflection;

namespace Pulsar.Web.Client.Services.PushNotifications;

public class SignalRManager(
	IPushNotificationClient pushNotificationClient,
	ILogger<SignalRManager> logger,
	IMediator mediator,
	List<Assembly> assembliesToScan) : IAsyncDisposable
{
	private bool _started = false;
	private bool _scanned = false;
	private Dictionary<PushNotificationKey, List<Type>> _eventsToFire = new ();
	private Dictionary<PushNotificationKey, List<EventHandler<PushNotificationEvent>>> _eventHandlersForKey = new();
	private Dictionary<Type, List<Delegate>> _eventHandlersForData = new();
	private HubConnection? _connection = null;

	public event EventHandler<PushNotificationEvent>? PushNotificationReceived;
	public event EventHandler? Reconnecting;
	public event EventHandler? Reconnected;
	public event EventHandler? Unconnected;
	public event EventHandler? Closed;

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
			await StartConnection();
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "error trying to estabilish a connection to SignalR push notification service");
			Unconnected?.Invoke(this, EventArgs.Empty);
		}
	}

	public async Task<bool> Reconnect()
	{
		try
		{
			await StartConnection();
			return true;
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "error retrying to estabilish a connection to SignalR push notification service");
			return false;
		}
	}

	private async Task StartConnection()
	{
		_started = false;
		if (_connection != null)
		{
			await TryDisposeConnection();
		}
		
		var session = await pushNotificationClient.StartSession();
		_connection = new HubConnectionBuilder()
			.WithUrl(session.Url, options =>
			{
				options.Headers.Add("Authorization", $"Bearer {session.Token}");
			})
			.Build();
			
		_connection.Closed += ConnectionOnClosed;
		_connection.On("published", (string pn) =>
		{
			OnPublish(mediator, pn.FromJsonString<PushNotificationDataWithId>());
		});

		await _connection.StartAsync();
		Console.WriteLine($"Connection started: {session.Url}, {session.Token}");
		_started = true;
	}

	private async Task TryDisposeConnection()
	{
		try
		{
			if (_connection == null)
			{
				return;
			}
			await _connection.DisposeAsync();
			_connection = null;
		}
		catch
		{
		}
	}

	private async Task ConnectionOnClosed(Exception? arg)
	{
		if (!_started)
		{
			return;
		}

		List<TimeSpan> timeouts = [
			TimeSpan.FromSeconds(0),
			TimeSpan.FromSeconds(5),
			TimeSpan.FromSeconds(30),
			TimeSpan.FromMinutes(1),
			TimeSpan.FromMinutes(5)];
		bool eventFired = false;
		bool reconnected = false;

		foreach (var timeout in timeouts)
		{
			await Task.Delay(timeout);
			try
			{
				if (!eventFired)
				{
					try
					{
						this.Reconnecting?.Invoke(this, EventArgs.Empty);
					}
					catch { }
					eventFired = true;
				}
				await StartConnection();

				reconnected = true;
				try
				{
					this.Reconnected?.Invoke(this, EventArgs.Empty);
				}
				catch { }
				break;
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "error retrying to estabilish a connection to SignalR push notification service");
			}
		}

		if (!reconnected)
		{
			try
			{
				this.Closed?.Invoke(this, EventArgs.Empty);
			}
			catch { }
		}
	}

	private void OnPublish(IMediator mediator, PushNotificationDataWithId? pn)
	{
		if (pn == null)
		{
			return;
		}

		try
		{
			mediator.Publish(new PushNotificationEvent(pn));
		}
		catch { }
		FireAdditionalEvents(pn);

		// Fire additional events
		try
		{
			PushNotificationReceived?.Invoke(this, new PushNotificationEvent(pn));
		}
		catch { }
		if (_eventHandlersForKey.ContainsKey(pn.Key))
		{
			foreach (var handler in _eventHandlersForKey[pn.Key].ToList())
			{
				try
				{
					handler(this, new PushNotificationEvent(pn));
				}
				catch { }
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
					try
					{
						mediator.Publish(notification);
					}
					catch { }

					if (_eventHandlersForData.ContainsKey(type))
					{
						var strongEvent = PushNotificationEvent.StronglyTyped(pn, type);
						foreach (var handler in _eventHandlersForData[type].ToList())
						{
							try
							{
								handler.DynamicInvoke(this, strongEvent);
							}
							catch { }
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
			_started = false;
			await _connection.StopAsync();
			await _connection.DisposeAsync();
			_connection = null;
		}
	}
}
