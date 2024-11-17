

using MongoDB.Bson;
using Pulsar.BuildingBlocks.DDD;
using Pulsar.Services.Identity.Contracts.DTOs;
using Pulsar.Services.PushNotification.API.Application.BaseTypes;
using Pulsar.Services.PushNotification.Contracts.DTOs;
using Pulsar.Services.PushNotification.Domain.Aggregates.PushNotifications;
using Pulsar.Services.Shared.PushNotifications;
using static MongoDB.Driver.WriteConcern;
using System.Linq.Expressions;

namespace Pulsar.Services.PushNotification.API.Application.Queries;

public class NotificacaoPushQueries : PushNotificationQueries, INotificacaoPushQueries
{
	public const int MAX_NOTIFICATIONS = 200;

    public NotificacaoPushQueries(PushNotificationQueriesContext ctx) : base(ctx)
    {
    }

	public async Task<List<NoficacaoPushDTO>> GetNoficacoes(string usuarioId, string? dominioId, string? estabelecimentoId, string? consistencyToken)
	{
		var uid = usuarioId.ToObjectId();
		var did = dominioId?.ToObjectId();
		var eid = estabelecimentoId?.ToObjectId();

		return await this.StartCausallyConsistentSectionAsync(async ct =>
		{
			var userContexts = (uid, did, eid) switch
			{
				(var u, null, null) => await UserContextsCollection.FindAsync(u => u.UsuarioId == uid && u.DominioId == null && u.EstabelecimentoId == null).ToListAsync(),
				(var u, var d, null) when d is not null => await UserContextsCollection.FindAsync(u => u.UsuarioId == uid && u.DominioId == d && u.EstabelecimentoId == null).ToListAsync(),
				(var u, _, var e) when e is not null => await UserContextsCollection.FindAsync(u => u.UsuarioId == uid && u.EstabelecimentoId == e).ToListAsync(),
				_ => new List<Domain.Aggregates.UserContexts.UserContext>()
			};
			var ucIds = userContexts.Select(uc => uc.Id).ToList();

			Expression<Func<NotificacaoPush, bool>> filter = 
				u => ucIds.Contains(u.UserContextId) && (u.Display == PushNotificationDisplay.All || u.Display == PushNotificationDisplay.NotificationCenter);
			
			var notifications = await NotificacoesPushCollection.FindAsync<NoficacaoPushDTO>(
				filter,
				new FindOptions<NotificacaoPush, NoficacaoPushDTO>()
				{
					Sort = Builders<NotificacaoPush>.Sort.Descending(x => x.CreatedOn),
					Projection = Builders<NotificacaoPush>.Projection.Expression(n => new NoficacaoPushDTO()
					{
						Actions = n.Actions.Select(a => new PushNotificationActionDTO() { 
							ButtonStyle = a.ButtonStyle,
							Intent = a.Intent,
							LinkText = a.LinkText,	
							Placement = a.Placement,
							RouteKey = a.RouteKey,
							Parameters = a.Parameters.Select(p => new PushNotificationActionParamDTO()
							{
								ParamKey = p.ParamKey,
								ParamValue = p.ParamValue,
							}).ToList()
						}).ToList(),
						PrimaryAction = n.PrimaryAction != null ? new PushNotificationActionDTO()
						{
							ButtonStyle = n.PrimaryAction.ButtonStyle,
							Intent = n.PrimaryAction.Intent,
							LinkText = n.PrimaryAction.LinkText,
							Placement = n.PrimaryAction.Placement,
							RouteKey = n.PrimaryAction.RouteKey,
							Parameters = n.PrimaryAction.Parameters.Select(p => new PushNotificationActionParamDTO()
							{
								ParamKey = p.ParamKey,
								ParamValue = p.ParamValue,
							}).ToList()
						} : null,
						CreatedOn = n.CreatedOn,
						Data = n.Data,
						Display = n.Display,
						Id = n.Id.ToString(),
						Intent = n.Intent,
						IsRead = n.IsRead,
						Key = n.Key,
						Message = n.Message,
						Title = n.Title,
					}),
					Limit = MAX_NOTIFICATIONS
				}).ToListAsync();

			return notifications;
		}, consistencyToken);
	}
}
