
namespace Pulsar.BuildingBlocks.EventBus.Contracts.PushNotifications;

public class PushNotificationTarget
{
	public PushNotificationTarget(string usuarioId, string? dominioId, string? estabelecimentoId, PushNotificationTargetMatch match)
	{
		UsuarioId = usuarioId;
		DominioId = dominioId;
		EstabelecimentoId = estabelecimentoId;
		Match = match;
	}

	public string UsuarioId { get; private set; }
	public string? DominioId { get; private set; }
	public string? EstabelecimentoId { get; private set; }
	public PushNotificationTargetMatch Match { get; private set; }

	internal PushNotificationTarget? Clone()
	{
		return new PushNotificationTarget(UsuarioId, DominioId, EstabelecimentoId, Match);
	}
}
