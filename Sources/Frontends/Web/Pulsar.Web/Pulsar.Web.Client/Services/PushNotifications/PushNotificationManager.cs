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

namespace Pulsar.Web.Client.Services.PushNotifications;

public class PushNotificationManager(
	PushNotificationService service, 
	IPushNotificationClient pushNotificationClient, 
	IMessageService messageService, 
	IToastService toastService) : IDisposable
{
	public const int MAX_NOTIFICATIONS = 200;
	public const string NOTIFICATION_CENTER_SECTION = "NOTIFICATION_CENTER_SECTION";
	public static readonly int TOAST_TIMEOUT = 8000; // 8s

	private List<NotificacaoPushDTO> _notifications = new List<NotificacaoPushDTO>();
	private int _unread = 0;

	public event EventHandler? OnNotificationListChanged;

	public async Task Start()
	{
		var t1 = service.Start();
		var t2 = pushNotificationClient.Get();

		await t1;
		_notifications.Clear();
		_notifications.AddRange(await t2);
		_notifications = _notifications.OrderByDescending(n => n.CreatedOn).ToList();
		_unread = _notifications.Count(n => !n.IsRead);
		service.OnPushNotification += Service_OnPushNotification;
		OnNotificationListChanged?.Invoke(this, EventArgs.Empty);

		messageService.Clear(NOTIFICATION_CENTER_SECTION);
		foreach (var notification in _notifications)
		{
			ShowPushNotification(notification, noToast: true);
		}
	}

	private void Service_OnPushNotification(object? sender, PushNotificationEvent e)
	{
		var notificacao = new NotificacaoPushDTO()
		{
			CreatedOn = e.PushNotificationData.CreatedOn,
			LabelAction = e.PushNotificationData.LabelAction != null ? new PushNotificationActionDTO()
			{
				RouteKey = e.PushNotificationData.LabelAction.RouteKey,
				Text = e.PushNotificationData.LabelAction.Text,
				Parameters = e.PushNotificationData.LabelAction.Parameters.Select(p => new PushNotificationActionParamDTO()
				{
					ParamKey = p.ParamKey,
					ParamValue = p.ParamValue,
				}).ToList()
			} : null,
			PrimaryAction = e.PushNotificationData.PrimaryAction != null ? new PushNotificationActionDTO()
			{
				RouteKey = e.PushNotificationData.PrimaryAction.RouteKey,
				Text = e.PushNotificationData.PrimaryAction.Text,
				Parameters = e.PushNotificationData.PrimaryAction.Parameters.Select(p => new PushNotificationActionParamDTO()
				{
					ParamKey = p.ParamKey,
					ParamValue = p.ParamValue,
				}).ToList()
			} : null,
			SecondaryAction = e.PushNotificationData.SecondaryAction != null ? new PushNotificationActionDTO()
			{
				RouteKey = e.PushNotificationData.SecondaryAction.RouteKey,
				Text = e.PushNotificationData.SecondaryAction.Text,
				Parameters = e.PushNotificationData.SecondaryAction.Parameters.Select(p => new PushNotificationActionParamDTO()
				{
					ParamKey = p.ParamKey,
					ParamValue = p.ParamValue,
				}).ToList()
			} : null,
			Data = e.PushNotificationData.Data,
			Display = e.PushNotificationData.Display,
			Id = e.PushNotificationData.PushNotificationId!,
			Intent = e.PushNotificationData.Intent,
			IsRead = false,
			Key = e.PushNotificationData.Key,
			Message = e.PushNotificationData.Message,
			Title = e.PushNotificationData.Title,
			ToastActionOptions = e.PushNotificationData.ToastActionOptions,
			ToastDisplayOptions = e.PushNotificationData.ToastDisplayOptions,
		};
		_notifications.Insert(0, notificacao);
		if (_notifications.Count > MAX_NOTIFICATIONS)
		{
			_notifications = _notifications.Take(MAX_NOTIFICATIONS).ToList();
		}
		_unread = _notifications.Count(n => !n.IsRead);
		OnNotificationListChanged?.Invoke(this, EventArgs.Empty);
		ShowPushNotification(notificacao);
	}

	public void Dispose()
	{
		service.OnPushNotification -= Service_OnPushNotification;
	}

	public IReadOnlyList<NotificacaoPushDTO> Notifications => _notifications.AsReadOnly();
	public int Unread => _unread;

	public async Task MarkAllRead()
	{
		List<string> mark = new List<string>();
		foreach (var notification in _notifications) {
			if (!notification.IsRead)
			{
				mark.Add(notification.Id);
			}
			notification.IsRead = true;
		}
		_unread = 0;
		OnNotificationListChanged?.Invoke(this, EventArgs.Empty);
		await pushNotificationClient.MarcarComoLida(new MarcarNotificacoesComoLidaCmd(mark));
	}

	private void ShowPushNotification(NotificacaoPushDTO notificacao, bool noToast = false)
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

	private void ShowNotificationCenter(NotificacaoPushDTO notificacao)
	{
		messageService.ShowMessageBar(options =>
		{
			options.Intent = GetMessageIntent(notificacao.Intent);
			options.Title = notificacao.Title ?? "";
			options.Body = notificacao.Message ?? "";
			options.Timestamp = notificacao.CreatedOn;
			options.Section = NOTIFICATION_CENTER_SECTION;
			options.Icon = GetMessageIcon(notificacao.Intent);
			options.Link = notificacao.LabelAction?.Text != null ?
				new ActionLink<Message>()
				{
					Href = "javascript:void(0);",
					Text = notificacao.LabelAction?.Text,
					OnClick = async _ =>
						{
							await HandleAction(notificacao, notificacao.LabelAction);
						}
				} : null;
			options.PrimaryAction = notificacao.PrimaryAction?.Text != null ?
				new ActionButton<Message>()
				{
					Text = notificacao.PrimaryAction?.Text,
					OnClick = async _ =>
					{
						await HandleAction(notificacao, notificacao.PrimaryAction);
					}
				} : null;
			options.SecondaryAction = notificacao.SecondaryAction?.Text != null ?
				new ActionButton<Message>()
				{
					Text = notificacao.SecondaryAction?.Text,
					OnClick = async _ =>
					{
						await HandleAction(notificacao, notificacao.SecondaryAction);
					}
				} : null;
		});
	}

	private Icon? GetMessageIcon(PushNotificationIntent? intent)
	{
		if (intent == null)
		{
			return null;
		}
		switch (intent.Value)
		{
			case PushNotificationIntent.None:
			case PushNotificationIntent.Error:
			case PushNotificationIntent.Warning:
			case PushNotificationIntent.Information:
			case PushNotificationIntent.Success:
				return null;
			case PushNotificationIntent.Flash:
				return new Icons.Regular.Size24.Flash();
			case PushNotificationIntent.Calendar:
				return new Icons.Regular.Size24.Calendar();
			case PushNotificationIntent.Upload:
				return new Icons.Regular.Size24.ArrowUpload();
			case PushNotificationIntent.Download:
				return new Icons.Regular.Size24.ArrowDownload();
			case PushNotificationIntent.Person:
				return new Icons.Regular.Size24.Person();
			case PushNotificationIntent.Alert:
				return new Icons.Regular.Size24.Alert();
			case PushNotificationIntent.Delete:
				return new Icons.Regular.Size24.Delete();
			case PushNotificationIntent.News:
				return new Icons.Regular.Size24.News();
			case PushNotificationIntent.Edit:
				return new Icons.Regular.Size24.Edit();
			case PushNotificationIntent.New:
				return new Icons.Regular.Size24.New();
			case PushNotificationIntent.Add:
				return new Icons.Regular.Size24.Add();
			case PushNotificationIntent.Heart:
				return new Icons.Regular.Size24.Heart();
			case PushNotificationIntent.Sync:
				return new Icons.Regular.Size24.ArrowSync();
			case PushNotificationIntent.Save:
				return new Icons.Regular.Size24.Save();
			case PushNotificationIntent.Folder:
				return new Icons.Regular.Size24.Folder();
			case PushNotificationIntent.Star:
				return new Icons.Regular.Size24.Star();
			case PushNotificationIntent.Mail:
				return new Icons.Regular.Size24.Mail();
			case PushNotificationIntent.Home:
				return new Icons.Regular.Size24.Home();
			default:
				return null;
		}
	}

	private MessageIntent? GetMessageIntent(PushNotificationIntent? intent)
	{
		if (intent == null)
		{
			return null;
		}

		switch (intent.Value)
		{
			case PushNotificationIntent.None:
				return null;
			case PushNotificationIntent.Error:
				return MessageIntent.Error;
			case PushNotificationIntent.Warning:
				return MessageIntent.Warning;
			case PushNotificationIntent.Information:
				return MessageIntent.Info;
			case PushNotificationIntent.Success:
				return MessageIntent.Success;
			default:
				return MessageIntent.Custom;
		}
	}

	private void ShowToast(NotificacaoPushDTO notificacao)
	{
		var displayOptions = notificacao.ToastDisplayOptions;
		if (displayOptions == null)
		{
			displayOptions = PushNotificationToastDisplayOptions.UseTitle;
		}
		if (notificacao.ToastActionOptions == PushNotificationToastActionOptions.UsePrimaryAndSecondaryAction)
		{
			displayOptions = PushNotificationToastDisplayOptions.UseBoth;
		}
		var useCommunication = displayOptions == PushNotificationToastDisplayOptions.UseBoth;
		if (useCommunication)
		{
			toastService.ShowCommunicationToast(new ToastParameters<CommunicationToastContent>()
			{
				Intent = GetToastIntent(notificacao.Intent),
				Icon = GetToastIcon(notificacao.Intent),
				Title = notificacao.Title,
				Timeout = TOAST_TIMEOUT,
				PrimaryAction = notificacao.PrimaryAction?.Text,
				OnPrimaryAction = notificacao.PrimaryAction?.Text != null ?
					EventCallback.Factory.Create<ToastResult>(this, async () => await HandleAction(notificacao, notificacao.PrimaryAction)) : null,
				SecondaryAction = notificacao.SecondaryAction?.Text,
				OnSecondaryAction = notificacao.SecondaryAction?.Text != null ? 
					EventCallback.Factory.Create<ToastResult>(this, async () => await HandleAction(notificacao, notificacao.SecondaryAction)) : null,
				Content = new CommunicationToastContent()
				{
					Details = notificacao.Message,
				}
			});
		}
		else
		{
			if (IsCustomToast(notificacao.Intent))
			{
				toastService.ShowCustom(
							GetToastTitle(displayOptions, notificacao),
							TOAST_TIMEOUT,
							GetToastAction(notificacao),
							GetToastCallback(notificacao),
							GetToastIcon(notificacao.Intent));
			}
			else
			{
				toastService.ShowToast(
					GetToastIntent(notificacao.Intent),
					GetToastTitle(displayOptions, notificacao),
					TOAST_TIMEOUT,
					GetToastAction(notificacao),
					GetToastCallback(notificacao));
			}
		}
	}

	private Task HandleAction(NotificacaoPushDTO notificacao, PushNotificationActionDTO? primaryAction)
	{
		throw new NotImplementedException();
	}

	private (Icon Value, Color Color)? GetToastIcon(PushNotificationIntent? intent)
	{
		if (intent == null)
		{
			return null;
		}
		switch (intent.Value)
		{
			case PushNotificationIntent.None:
			case PushNotificationIntent.Error:
			case PushNotificationIntent.Warning:
			case PushNotificationIntent.Information:
			case PushNotificationIntent.Success:
			case PushNotificationIntent.Flash:
			case PushNotificationIntent.Calendar:
			case PushNotificationIntent.Upload:
			case PushNotificationIntent.Download:
			case PushNotificationIntent.Person:
				return null;
			case PushNotificationIntent.Alert:
				return (new Icons.Regular.Size24.Alert(), Color.Accent);
			case PushNotificationIntent.Delete:
				return (new Icons.Regular.Size24.Delete(), Color.Accent);
			case PushNotificationIntent.News:
				return (new Icons.Regular.Size24.News(), Color.Accent);
			case PushNotificationIntent.Edit:
				return (new Icons.Regular.Size24.Edit(), Color.Accent);
			case PushNotificationIntent.New:
				return (new Icons.Regular.Size24.New(), Color.Accent);
			case PushNotificationIntent.Add:
				return (new Icons.Regular.Size24.Add(), Color.Accent);
			case PushNotificationIntent.Heart:
				return (new Icons.Regular.Size24.Heart(), Color.Accent);
			case PushNotificationIntent.Sync:
				return (new Icons.Regular.Size24.ArrowSync(), Color.Accent);
			case PushNotificationIntent.Save:
				return (new Icons.Regular.Size24.Save(), Color.Accent);
			case PushNotificationIntent.Folder:
				return (new Icons.Regular.Size24.Folder(), Color.Accent);
			case PushNotificationIntent.Star:
				return (new Icons.Regular.Size24.Star(), Color.Accent);
			case PushNotificationIntent.Mail:
				return (new Icons.Regular.Size24.Mail(), Color.Accent);
			case PushNotificationIntent.Home:
				return (new Icons.Regular.Size24.Home(), Color.Accent);
			default:
				return null;
		}
	}

	private EventCallback<ToastResult>? GetToastCallback(NotificacaoPushDTO notificacao)
	{
		if (notificacao.ToastActionOptions == null)
			return null;

		if (notificacao.ToastActionOptions == PushNotificationToastActionOptions.NoAction)
			return null;
		else if (notificacao.ToastActionOptions == PushNotificationToastActionOptions.UsePrimaryAction)
			return EventCallback.Factory.Create<ToastResult>(this, () => HandleAction(notificacao, notificacao.PrimaryAction));
		else if (notificacao.ToastActionOptions == PushNotificationToastActionOptions.UseSecondaryAction)
			return EventCallback.Factory.Create<ToastResult>(this, () => HandleAction(notificacao, notificacao.SecondaryAction));
		else if (notificacao.ToastActionOptions == PushNotificationToastActionOptions.UseLabel)
			return EventCallback.Factory.Create<ToastResult>(this, () => HandleAction(notificacao, notificacao.LabelAction));
		else
			return null;
	}

	private string? GetToastAction(NotificacaoPushDTO notificacao)
	{
		if (notificacao.ToastActionOptions == null)
			return null;

		if (notificacao.ToastActionOptions == PushNotificationToastActionOptions.NoAction)
			return null;
		else if (notificacao.ToastActionOptions == PushNotificationToastActionOptions.UsePrimaryAction)
			return notificacao.PrimaryAction?.Text;
		else if (notificacao.ToastActionOptions == PushNotificationToastActionOptions.UseSecondaryAction)
			return notificacao.SecondaryAction?.Text;
		else if (notificacao.ToastActionOptions == PushNotificationToastActionOptions.UseLabel)
			return notificacao.LabelAction?.Text;
		else
			return null;
	}

	private string GetToastTitle(PushNotificationToastDisplayOptions? displayOptions, NotificacaoPushDTO notificacao)
	{
		if (displayOptions == null)
		{
			return notificacao.Title ?? "";
		}
		switch (displayOptions.Value)
		{
			case PushNotificationToastDisplayOptions.UseTitle:
				return notificacao.Title ?? "";
			default:
				return notificacao.Message ?? "";
		}
	}

	private bool IsCustomToast(PushNotificationIntent? intent)
	{
		if (intent == null)
		{
			return true;
		}
		switch (intent.Value)
		{
			case PushNotificationIntent.None:
			case PushNotificationIntent.Error:
			case PushNotificationIntent.Warning:
			case PushNotificationIntent.Information:
			case PushNotificationIntent.Success:
			case PushNotificationIntent.Flash:
			case PushNotificationIntent.Calendar:
			case PushNotificationIntent.Upload:
			case PushNotificationIntent.Download:
			case PushNotificationIntent.Person:
				return false;
			default:
				return true;
		}
	}

	private ToastIntent GetToastIntent(PushNotificationIntent? intent)
	{
		if (intent == null)
			return ToastIntent.Custom;

		switch (intent.Value!)
		{
			case PushNotificationIntent.None:
				return ToastIntent.Custom;
			case PushNotificationIntent.Error:
				return ToastIntent.Error;
			case PushNotificationIntent.Warning:
				return ToastIntent.Warning;
			case PushNotificationIntent.Information:
				return ToastIntent.Info;
			case PushNotificationIntent.Success:
				return ToastIntent.Success;
			case PushNotificationIntent.Flash:
				return ToastIntent.Progress;
			case PushNotificationIntent.Calendar:
				return ToastIntent.Event;
			case PushNotificationIntent.Upload:
				return ToastIntent.Upload;
			case PushNotificationIntent.Download:
				return ToastIntent.Download;
			case PushNotificationIntent.Person:
				return ToastIntent.Mention;
			default:
				return ToastIntent.Custom;
		}
	}
}
