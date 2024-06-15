using MediatR;
using Microsoft.AspNetCore.Mvc;
using Pulsar.Services.Identity.Contracts.Queries;

namespace Pulsar.Services.Catalog.API.Utils;

public class CatalogController : ControllerBase
{
    private readonly CatalogControllerContext _context;
    public IMediator Mediator => _context.Mediator;
    public IConfiguration Configuration => _context.Configuration;
    //public IUsuarioQueries UsuarioQueries => _context.UsuarioQueries;
    //public IDominioQueries DominioQueries => _context.DominioQueries;
    //public IGrupoQueries GrupoQueries => _context.GrupoQueries;
    //public IEstabelecimentoQueries EstabelecimentoQueries => _context.EstabelecimentoQueries;
    public CatalogController(CatalogControllerContext context)
    {
        _context = context;
    }
}

public class CatalogControllerContext
{
    public IMediator Mediator { get; }
    //public IUsuarioQueries UsuarioQueries { get; }
    //public IDominioQueries DominioQueries { get; }
    //public IEstabelecimentoQueries EstabelecimentoQueries { get; }
    //public IGrupoQueries GrupoQueries { get; }
    public IConfiguration Configuration { get; }

    public CatalogControllerContext(IMediator mediator, IConfiguration configuration)
    {
        Mediator = mediator;
        //UsuarioQueries = usuarioQueries;
        //DominioQueries = dominioQueries;
        //EstabelecimentoQueries = estabelecimentoQueries;
        //GrupoQueries = grupoQueries;
        Configuration = configuration;
    }
}
