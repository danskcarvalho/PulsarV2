using MongoDB.Bson;
using Pulsar.BuildingBlocks.DDD;
using Pulsar.BuildingBlocks.DDD.Attributes;
using Pulsar.Services.PushNotification.Contracts.Commands.Sessions;
using Pulsar.Services.PushNotification.Domain.Aggregates.Sessions;
using Pulsar.Services.Shared.Commands;

namespace Pulsar.Services.PushNotification.API.Application.Commands.Sessions;

[RetryOnException(DuplicatedKey = true)]
public class CriarSessaoCH : PushNotificationCommandHandler<CriarSessaoCmd, CriarSessaoResult>
{
	public CriarSessaoCH(PushNotificationCommandHandlerContext<CriarSessaoCmd, CriarSessaoResult> ctx) : base(ctx)
	{
	}

	protected override async Task<CriarSessaoResult> HandleAsync(CriarSessaoCmd cmd, CancellationToken ct)
	{
		var session = new Session(
			ObjectId.GenerateNewId(),
			cmd.UsuarioId.ToObjectId(),
			cmd.DominioId?.ToObjectId(),
			cmd.EstabelecimentoId?.ToObjectId(),
			GeneralExtensions.GetSalt(32));
		session.Criar();
		await SessionRepository.InsertOneAsync(session);
		return new CriarSessaoResult(session.Id.ToString(), session.Token);
	}
}
