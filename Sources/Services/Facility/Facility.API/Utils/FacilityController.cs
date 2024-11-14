namespace Pulsar.Services.Facility.API.Utils;

public class FacilityController : ControllerBase
{
    private readonly FacilityControllerContext _context;
    public IMediator Mediator => _context.Mediator;
    public IConfiguration Configuration => _context.Configuration;
    public IEstabelecimentoQueries EstabelecimentoQueries => _context.EstabelecimentoQueries;
    public FacilityController(FacilityControllerContext context)
    {
        _context = context;
    }
}
