using MediatR;

namespace Pulsar.Services.PushNotification.API.Utils;

public class BaseControllerContext(IMediator mediator,
                                      INotificacoesPushQueries estabelecimentoQueries,
                                      IConfiguration configuration)
{
    public IMediator Mediator => mediator;
    public INotificacoesPushQueries EstabelecimentoQueries => estabelecimentoQueries;
    public IConfiguration Configuration => configuration;
}
