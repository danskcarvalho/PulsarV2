using MongoDB.Bson;
using Pulsar.Services.PushNotification.Domain.Aggregates.PushNotifications;

namespace Pulsar.Services.PushNotification.Domain.Specifications.PushNotifications;

public class MarcarNotificacoesComoLidaSpec : IUpdateSpecification<NotificacaoPush>
{
	public MarcarNotificacoesComoLidaSpec(List<ObjectId> notificacoes, ObjectId usuarioId)
	{
		Notificacoes = notificacoes;
		UsuarioId = usuarioId;
	}

	public List<ObjectId> Notificacoes { get; set; }
	public ObjectId UsuarioId { get; set; }

	public UpdateSpecification<NotificacaoPush> GetSpec()
	{
		return
			Update
			.Where<NotificacaoPush>(n => n.UsuarioId == UsuarioId && Notificacoes.Contains(n.Id))
			.Set(n => n.IsRead, true)
			.Build();
	}
}
