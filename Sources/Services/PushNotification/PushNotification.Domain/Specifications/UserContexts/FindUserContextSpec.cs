using MongoDB.Bson;
using Pulsar.Services.PushNotification.Domain.Aggregates.UserContexts;

namespace Pulsar.Services.PushNotification.Domain.Specifications.UserContexts;

public class FindUserContextSpec : IFindSpecification<UserContext>
{
	public FindUserContextSpec(ObjectId usuarioId, ObjectId? dominioId, ObjectId? estabelecimentoId)
	{
		UsuarioId = usuarioId;
		DominioId = dominioId;
		EstabelecimentoId = estabelecimentoId;
	}

	public ObjectId UsuarioId { get; private set; }
	public ObjectId? DominioId { get; private set; }
	public ObjectId? EstabelecimentoId { get; private set; }

	public FindSpecification<UserContext> GetSpec()
	{
		return Find.Where<UserContext>(u => u.UsuarioId == UsuarioId && u.DominioId == DominioId && u.EstabelecimentoId == EstabelecimentoId).Build();
	}
}
