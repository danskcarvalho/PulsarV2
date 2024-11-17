namespace Pulsar.Services.PushNotification.Contracts.Commands.PushNotifications;

public class MarcarNotificacoesComoLidaCmd : IRequest<CommandResult>
{
	[JsonConstructor]
	public MarcarNotificacoesComoLidaCmd(List<string> notificacoes, string usuarioId)
	{
		Notificacoes = notificacoes;
		UsuarioId = usuarioId;
	}

	public List<string> Notificacoes { get; set; }
	public string UsuarioId { get; set; }
}
