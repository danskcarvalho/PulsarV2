using Pulsar.Services.PushNotification.Contracts.Commands.UserContexts;
using Pulsar.Services.PushNotification.Domain.Specifications.Sessions;
using Pulsar.Services.PushNotification.Domain.Specifications.UserContexts;

namespace Pulsar.Services.PushNotification.Functions.Application.Commands.UserContexts;

[NoTransaction]
public class GetUserContextByTokenCH : PushNotificationCommandHandler<GetUserContextByTokenCmd, GetUserContextByTokenResult>
{
	public GetUserContextByTokenCH(PushNotificationCommandHandlerContext<GetUserContextByTokenCmd, GetUserContextByTokenResult> ctx) : base(ctx)
	{
	}

	protected override async Task<GetUserContextByTokenResult> HandleAsync(GetUserContextByTokenCmd cmd, CancellationToken ct)
	{
		var session = await SessionRepository.FindOneAsync(new GetSessionByTokenSpec(cmd.Token));
		if (session == null)
		{
			return new GetUserContextByTokenResult(null);
		}
		var userContext = await UserContextRepository.FindOneAsync(new FindUserContextSpec(session.UsuarioId, session.DominioId, session.EstabelecimentoId));
		return new GetUserContextByTokenResult(userContext?.Id);
	}
}
