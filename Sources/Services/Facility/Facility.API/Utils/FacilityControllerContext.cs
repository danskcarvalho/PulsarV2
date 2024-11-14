using MediatR;

namespace Pulsar.Services.Facility.API.Utils;

public class FacilityControllerContext(IMediator mediator,
                                      IEstabelecimentoQueries estabelecimentoQueries,
                                      IConfiguration configuration)
{
    public IMediator Mediator => mediator;
    public IEstabelecimentoQueries EstabelecimentoQueries => estabelecimentoQueries;
    public IConfiguration Configuration => configuration;
}
