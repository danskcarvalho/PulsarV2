using Microsoft.AspNetCore.Authorization;
using System.Globalization;

namespace Pulsar.Services.Identity.API.Authorization;

public class EstabelecimentoAuthorizeAttribute : AuthorizeAttribute
{
    public EstabelecimentoAuthorizeAttribute(params PermissoesEstabelecimento[] permissoes)
    {
        AuthenticationSchemes = "Bearer,MockedAuthScheme";
        Permissoes = permissoes;
        Policy = "Estabelecimento_" + string.Join('_', permissoes.Select(x => (int)x).Select(x => x.ToString(CultureInfo.InvariantCulture)).ToArray());
    }

    public PermissoesEstabelecimento[] Permissoes { get; }
}
