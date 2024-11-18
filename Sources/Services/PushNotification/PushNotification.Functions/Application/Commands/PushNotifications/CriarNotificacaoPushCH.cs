using Pulsar.Services.PushNotification.Contracts.Commands.PushNotifications;
using Pulsar.Services.PushNotification.Domain.Aggregates.PushNotifications;
using Pulsar.Services.PushNotification.Domain.Specifications.UserContexts;

namespace Pulsar.Services.PushNotification.Functions.Application.Commands.PushNotifications;

[NoTransaction]
public class CriarNotificacaoPushCH : PushNotificationCommandHandler<CriarNotificacaoPushCmd, CriarNotificacaoPushResult>
{
	public CriarNotificacaoPushCH(PushNotificationCommandHandlerContext<CriarNotificacaoPushCmd, CriarNotificacaoPushResult> ctx) : base(ctx)
	{
	}

	protected override async Task<CriarNotificacaoPushResult> HandleAsync(CriarNotificacaoPushCmd cmd, CancellationToken ct)
	{
		if(cmd.PushNotificationData.Target == null)
		{
			return new CriarNotificacaoPushResult(null, null);
		}
		var userContextIds = (await UserContextRepository.FindManyAsync(new FindUserContextsByTargetSpec(cmd.PushNotificationData.Target))).Select(u => (u.Id, u.UsuarioId)).ToList();
		List<(string PushNotificationId, string UserContextId)> targets = new List<(string PushNotificationId, string UserContextId)>();
		List<NotificacaoPush> pushNotifications = new List<NotificacaoPush>();
		foreach (var userContext in userContextIds)
		{
			var pn = new NotificacaoPush(userContext.Id, userContext.UsuarioId, cmd.PushNotificationData);
			pushNotifications.Add(pn);
			targets.Add((pn.Id.ToString(), pn.UserContextId.ToString()));
		}
		await PushNotificationRepository.InsertManyAsync(pushNotifications);
		return new CriarNotificacaoPushResult(cmd.PushNotificationData.Clone(), targets);
	}
}
