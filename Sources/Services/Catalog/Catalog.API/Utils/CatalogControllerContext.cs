using MediatR;
using Pulsar.Services.Catalog.Contracts.Queries;

namespace Pulsar.Services.Catalog.API.Utils;

public class CatalogControllerContext(IMediator mediator,
                                      IDenteQueries denteQueries,
                                      IDiagnosticoQueries diagnosticosQueries,
                                      IEspecialidadeQueries especialidadeQueries,
                                      IEtniaQueries etniaQueries,
                                      IInepQueries inepQueries,
                                      IMaterialQueries materialQueries,
                                      IPrincipioAtivoQueries principioAtivoQueries,
                                      IProcedimentoQueries procedimentoQueries,
                                      IRegiaoQueries regiaoQueries,
                                      IConfiguration configuration)
{
    public IMediator Mediator => mediator;
    public IDenteQueries DenteQueries => denteQueries;
    public IConfiguration Configuration => configuration;
    public IDiagnosticoQueries DiagnosticosQueries => diagnosticosQueries;
    public IEspecialidadeQueries EspecialidadeQueries => especialidadeQueries;
    public IEtniaQueries EtniaQueries => etniaQueries;
    public IInepQueries InepQueries => inepQueries;
    public IMaterialQueries MaterialQueries => materialQueries;
    public IPrincipioAtivoQueries PrincipioAtivoQueries => principioAtivoQueries;
    public IProcedimentoQueries ProcedimentoQueries => procedimentoQueries;
    public IRegiaoQueries RegiaoQueries => regiaoQueries;
}
