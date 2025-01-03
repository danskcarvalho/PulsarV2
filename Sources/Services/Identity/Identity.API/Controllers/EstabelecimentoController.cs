﻿namespace Pulsar.Services.Identity.API.Controllers;

[ApiController]
[Route("v2/estabelecimentos")]
[Authorize(Policy = "InferAuthenticationSchemes")]
public class EstabelecimentoController : IdentityController
{
    public EstabelecimentoController(IdentityControllerContext context) : base(context)
    {
    }

    /// <summary>
    /// Retorna os detalhes do estabelecimento logado.
    /// </summary>
    /// <returns></returns>
    [HttpGet("logado"), ScopeAuthorize("identity.estabelecimentos.logado")]
    public async Task<ActionResult<IdNomeViewModel>> Logado()
    {
        var estabelecimentoId = User.EstabelecimentoId();
        if (estabelecimentoId == null)
            throw new IdentityDomainException(IdentityExceptionKey.EstabelecimentoNaoLogado);
        var r = await EstabelecimentoQueries.GetEstabelecimentoLogado(estabelecimentoId);
        return Ok(r);
    }
}
