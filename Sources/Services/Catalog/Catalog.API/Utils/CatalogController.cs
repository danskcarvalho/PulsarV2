using MediatR;
using Microsoft.AspNetCore.Mvc;
using Pulsar.Services.Catalog.API.Application.Queries;
using Pulsar.Services.Catalog.Contracts.Queries;
using Pulsar.Services.Identity.Contracts.Queries;

namespace Pulsar.Services.Catalog.API.Utils;

public class CatalogController : ControllerBase
{
    private readonly CatalogControllerContext _context;
    public IMediator Mediator => _context.Mediator;
    public IConfiguration Configuration => _context.Configuration;
    public IDenteQueries DenteQueries => _context.DenteQueries;
    public IDiagnosticoQueries DiagnosticoQueries => _context.DiagnosticosQueries;
    public IEspecialidadeQueries EspecialidadeQueries => _context.EspecialidadeQueries;
    public IEtniaQueries EtniaQueries => _context.EtniaQueries;
    public IInepQueries InepQueries => _context.InepQueries;
    public IMaterialQueries MaterialQueries => _context.MaterialQueries;
    public IPrincipioAtivoQueries PrincipioAtivoQueries => _context.PrincipioAtivoQueries;
    public IProcedimentoQueries ProcedimentoQueries => _context.ProcedimentoQueries;
    public IRegiaoQueries RegiaoQueries => _context.RegiaoQueries;
    public CatalogController(CatalogControllerContext context)
    {
        _context = context;
    }
}
