using MongoDB.Bson;
using Pulsar.BuildingBlocks.DDD.Attributes;
using Pulsar.Services.PushNotification.Domain.Aggregates.UserContexts;
using Pulsar.Services.PushNotification.Domain.Events.PushNotifications;
using Pulsar.Services.PushNotification.Domain.Specifications.UserContexts;

namespace Pulsar.Services.PushNotification.API.Application.DomainEvents.UserContexts;

[RetryOnException(DuplicatedKey = true)]
public class CriarUserContextDEH : PushNotificationDomainEventHandler<SessaoCriadaDE>
{
	public CriarUserContextDEH(PushNotificationDomainEventHandlerContext<SessaoCriadaDE> ctx) : base(ctx)
	{
	}

	protected override async Task HandleAsync(SessaoCriadaDE evt, CancellationToken ct)
	{
		var spec = new FindUserContextSpec(evt.UsuarioId, evt.EstabelecimentoId != null ? null : evt.DominioId, evt.EstabelecimentoId);
		var userContext = await UserContextRepository.FindOneAsync(spec);
		if (userContext != null)
		{
			return;
		}
		await UserContextRepository.InsertOneAsync(
			new UserContext(ObjectId.GenerateNewId(),
				   evt.UsuarioId,
				   evt.EstabelecimentoId != null ? null : evt.DominioId,
				   evt.EstabelecimentoId));
	}
}
