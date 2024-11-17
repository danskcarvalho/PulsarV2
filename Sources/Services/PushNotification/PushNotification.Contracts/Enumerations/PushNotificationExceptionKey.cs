using System.ComponentModel.DataAnnotations;

namespace Pulsar.Services.PushNotification.Contracts.Enumerations;

public enum PushNotificationExceptionKey
{
	[Display(Description = "Notificação não encontrada.")]
	NotificaoNaoEncontrada
}
