using MediatR;

namespace Pulsar.Services.PushNotification.API.Utils;

public class BaseControllerContext(IMediator mediator,
                                      IPushNotificationQueries estabelecimentoQueries,
                                      IConfiguration configuration)
{
    public IMediator Mediator => mediator;
    public IPushNotificationQueries EstabelecimentoQueries => estabelecimentoQueries;
    public IConfiguration Configuration => configuration;
}
