using MediatR;
using Microsoft.AspNetCore.Mvc;
using Pulsar.Services.Catalog.Contracts.Queries;
using Pulsar.Services.Identity.Contracts.Queries;

namespace Pulsar.Services.Catalog.API.Utils;

public class CatalogController : ControllerBase
{
    private readonly CatalogControllerContext _context;
    public IMediator Mediator => _context.Mediator;
    public IConfiguration Configuration => _context.Configuration;
    public IDenteQueries DenteQueries => _context.DenteQueries;
    public CatalogController(CatalogControllerContext context)
    {
        _context = context;
    }
}

public class CatalogControllerContext
{
    public IMediator Mediator { get; }
    public IDenteQueries DenteQueries { get; }
    public IConfiguration Configuration { get; }

    public CatalogControllerContext(IMediator mediator, IDenteQueries denteQueries, IConfiguration configuration)
    {
        Mediator = mediator;
        DenteQueries = denteQueries;
        Configuration = configuration;
    }
}
