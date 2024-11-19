using Pulsar.BuildingBlocks.EventBus.Contracts;
using Pulsar.BuildingBlocks.EventBus.Contracts.PushNotifications;
using Pulsar.BuildingBlocks.EventBus.Events;
using Pulsar.Services.Shared;
using Pulsar.Services.Shared.PushNotifications;

namespace Pulsar.Services.Identity.Contracts.IntegrationEvents;

[EventName("Identity:ConviteAceitoIE")]
[PushNotificationEvent(PushNotificationKey.ConviteAceito)]
public record ConviteAceitoIE : IntegrationEvent, IPushNotificationEvent
{
	/// <summary>
	/// Usuario que aceito convite.
	/// </summary>
	public required string UsuarioId { get; init; }
	public required string UsuarioEmail { get; init; }
	public required string UsuarioConvidanteId { get; init; }
	public required string UsuarioConvidanteEmail { get; init; }

	public PushNotificationData? GetPushNotificationData()
	{
		return new PushNotificationData(
			PushNotificationKey.ConviteAceito, 
			new PushNotificationTarget(UsuarioConvidanteId, null, null, PushNotificationTargetMatch.MatchUsuarioOnly),
			DateTime.UtcNow)
		{
			Data = this.ToJsonString(),
			Display = PushNotificationDisplay.All,
			Intent = PushNotificationIntent.Person,
			Message = $"Usuário {UsuarioEmail} aceitou o convite para participar do Pulsar.",
			Title = "Convite Aceito"
		};
	}
}
