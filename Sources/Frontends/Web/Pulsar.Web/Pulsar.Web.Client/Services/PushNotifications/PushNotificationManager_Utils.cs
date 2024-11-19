using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using Pulsar.BuildingBlocks.EventBus.Contracts.PushNotifications;
using Pulsar.Services.PushNotification.Contracts.DTOs;
using Pulsar.Services.Shared.PushNotifications;

namespace Pulsar.Web.Client.Services.PushNotifications;

public partial class PushNotificationManager
{
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

	private void ShowToast(PushNotificationDataWithId notificacao)
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

	private void ShowNotificationCenter(PushNotificationDataWithId notificacao)
	{
		if (!notificacao.IsRead)
		{
			messageService.ShowMessageBar(options =>
			{
				options.AllowDismiss = true;
				options.OnClose = async _ =>
				{
					await MarkAsRead(notificacao.PushNotificationId);
				};
				options.Intent = GetMessageIntent(notificacao.Intent);
				options.Title = notificacao.Title ?? "";
				options.Body = notificacao.Message ?? "";
				options.Timestamp = notificacao.CreatedOn;
				options.Section = NOTIFICATION_CENTER_SECTION_UNREAD;
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

		messageService.ShowMessageBar(options =>
		{
			options.AllowDismiss = false;
			options.Intent = GetMessageIntent(notificacao.Intent);
			options.Title = notificacao.Title ?? "";
			options.Body = notificacao.Message ?? "";
			options.Timestamp = notificacao.CreatedOn;
			options.Section = NOTIFICATION_CENTER_SECTION_HISTORY;
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

	private async Task HandleAction(PushNotificationDataWithId notificacao, PushNotificationDataAction? action)
	{
		if (action == null)
		{
			return;
		}

		var evt = new PushNotificationActionEvent(notificacao, action);
		await mediator.Send(evt);
		if (_actionHandlers.ContainsKey(action.RouteKey))
		{
			foreach (var handler in _actionHandlers[action.RouteKey].ToList())
			{
				handler.Invoke(this, evt);
			}
		}
		if (_routeHandlers.ContainsKey(action.RouteKey))
		{
			var uri = GetUri(_routeHandlers[action.RouteKey].Route, action);
			navigationManager.NavigateTo(uri);
		}
	}

	private string GetUri(string route, PushNotificationDataAction action)
	{
		foreach (var p in action.Parameters)
		{
			route = route.Replace($"{{{p.ParamKey.ToString()}}}", Uri.EscapeDataString(p.ParamValue));
		}
		return route;
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

	private EventCallback<ToastResult>? GetToastCallback(PushNotificationDataWithId notificacao)
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

	private string? GetToastAction(PushNotificationDataWithId notificacao)
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

	private string GetToastTitle(PushNotificationToastDisplayOptions? displayOptions, PushNotificationDataWithId notificacao)
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
