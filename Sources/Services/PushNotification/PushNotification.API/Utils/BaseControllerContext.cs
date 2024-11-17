using MediatR;

namespace Pulsar.Services.PushNotification.API.Utils;

public class BaseControllerContext(IMediator mediator,
                                      INotificacaoPushQueries notificacoesQueries,
                                      IConfiguration configuration)
{
    public IMediator Mediator => mediator;
    public INotificacaoPushQueries NotificacoesPushQueries => notificacoesQueries;
    public IConfiguration Configuration => configuration;
}
