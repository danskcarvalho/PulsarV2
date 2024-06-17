using MediatR;
using Pulsar.Services.Catalog.Contracts.Queries;

namespace Pulsar.Services.Catalog.API.Utils;

public class CatalogControllerContext(IMediator mediator, IDenteQueries denteQueries, IDiagnosticoQueries diagnosticosQueries, IConfiguration configuration)
{
    public IMediator Mediator => mediator;
    public IDenteQueries DenteQueries => denteQueries;
    public IConfiguration Configuration => configuration;
    public IDiagnosticoQueries DiagnosticosQueries => diagnosticosQueries;
}
