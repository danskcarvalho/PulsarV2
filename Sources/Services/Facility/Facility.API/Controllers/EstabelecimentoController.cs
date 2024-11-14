using Microsoft.AspNetCore.Mvc;
using Pulsar.Services.Facility.API.Utils;

namespace Pulsar.Services.Facility.API.Controllers;

[ApiController]
[Route("v2/estabelecimentos")]
[Authorize(Policy = "InferAuthenticationSchemes")]
public class EstabelecimentoController : FacilityController
{
    public EstabelecimentoController(FacilityControllerContext context) : base(context)
    {
    }
}
