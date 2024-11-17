using Pulsar.BuildingBlocks.DDD;
using Pulsar.BuildingBlocks.DDD.Attributes;
using Pulsar.Services.PushNotification.Contracts.Commands.PushNotifications;
using Pulsar.Services.PushNotification.Contracts.Enumerations;
using Pulsar.Services.PushNotification.Domain.Specifications.PushNotifications;
using Pulsar.Services.Shared.Commands;

namespace Pulsar.Services.PushNotification.API.Application.Commands.PushNotifications
{
	[NoTransaction, RetryOnException(VersionConcurrency = true)]
	public class MarcarNotificacoesComoLidaCH : PushNotificationCommandHandler<MarcarNotificacoesComoLidaCmd, CommandResult>
	{
		public MarcarNotificacoesComoLidaCH(PushNotificationCommandHandlerContext<MarcarNotificacoesComoLidaCmd, CommandResult> ctx) : base(ctx)
		{
		}

		protected override async Task<CommandResult> HandleAsync(MarcarNotificacoesComoLidaCmd cmd, CancellationToken ct)
		{
			var spec = new MarcarNotificacoesComoLidaSpec(cmd.Notificacoes.Select(n => n.ToObjectId()).ToList(), cmd.UsuarioId.ToObjectId());
			await NotificacaoPushRepository.UpdateManyAsync(spec).CheckModified();
			return new CommandResult();
		}
	}
}
