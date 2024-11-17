using MediatR;
using MongoDB.Bson;

namespace Pulsar.Services.PushNotification.Domain.Events.PushNotifications
{
	public class SessaoCriadaDE : INotification
	{
		public SessaoCriadaDE(ObjectId sessionId, ObjectId usuarioId, ObjectId? dominioId, ObjectId? estabelecimentoId)
		{
			SessionId = sessionId;
			UsuarioId = usuarioId;
			DominioId = dominioId;
			EstabelecimentoId = estabelecimentoId;
		}

		public ObjectId SessionId { get; set; }
		public ObjectId UsuarioId { get; set; }
		public ObjectId? DominioId { get; set; }
		public ObjectId? EstabelecimentoId { get; set; }
	}
}
