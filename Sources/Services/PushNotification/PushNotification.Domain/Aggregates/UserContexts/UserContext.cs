using MongoDB.Bson;
using Pulsar.BuildingBlocks.DDD;

namespace Pulsar.Services.PushNotification.Domain.Aggregates.UserContexts;

public partial class UserContext : AggregateRoot
{
	public UserContext(ObjectId id, ObjectId usuarioId, ObjectId? dominioId, ObjectId? estabelecimentoId) : base(id)
	{
		UsuarioId = usuarioId;
		DominioId = dominioId;
		EstabelecimentoId = estabelecimentoId;
	}

	public ObjectId UsuarioId { get; private set; }
	public ObjectId? DominioId { get; private set; }
	public ObjectId? EstabelecimentoId { get; private set; }

}
