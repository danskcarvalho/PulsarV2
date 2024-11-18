using Pulsar.BuildingBlocks.DDD;
using Pulsar.BuildingBlocks.EventBus.Contracts.PushNotifications;
using Pulsar.Services.PushNotification.Domain.Aggregates.UserContexts;

namespace Pulsar.Services.PushNotification.Domain.Specifications.UserContexts;

public class FindUserContextsByTargetSpec : IFindSpecification<UserContext>
{
	public FindUserContextsByTargetSpec(PushNotificationTarget target)
	{
		Target = target;
	}

	public PushNotificationTarget Target { get; private init; }

	public FindSpecification<UserContext> GetSpec()
	{
		var uid = Target.UsuarioId.ToObjectId();
		var did = Target.DominioId?.ToObjectId();
		var eid = Target.EstabelecimentoId?.ToObjectId();
		switch (Target.Match)
		{
			case PushNotificationTargetMatch.ExactMatch:
				if (did == null && eid == null)
				{
					return Find.Where<UserContext>(u => false).Build();
				}
				else
				{
					return did != null ?
						Find.Where<UserContext>(u => u.UsuarioId == uid && u.DominioId == did && u.EstabelecimentoId == eid).Build() :
						Find.Where<UserContext>(u => u.UsuarioId == uid && u.EstabelecimentoId == eid).Build();
				}
			case PushNotificationTargetMatch.MatchUsuarioOnly:
				return Find.Where<UserContext>(u => u.UsuarioId == uid).Build();
			case PushNotificationTargetMatch.MatchUsuarioDominio:
				return Find.Where<UserContext>(u => u.UsuarioId == uid && u.DominioId == did && u.EstabelecimentoId == null).Build();
			case PushNotificationTargetMatch.MatchUsuarioEstabelecimentos:
				return Find.Where<UserContext>(u => u.UsuarioId == uid && u.EstabelecimentoId != null).Build();
			case PushNotificationTargetMatch.MatchUsuarioEstabelecimentosFromDominio:
				return Find.Where<UserContext>(u => u.UsuarioId == uid && u.DominioId == did && u.EstabelecimentoId != null).Build();
			default:
				return Find.Where<UserContext>(u => false).Build();
		}
	}
}
