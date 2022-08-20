using Microsoft.AspNetCore.Authorization;
using System.Globalization;

namespace Pulsar.Services.Identity.API.Authorization;

public class SuperUsuarioOrDominioAuthorizeAttribute : AuthorizeAttribute
{
    public SuperUsuarioOrDominioAuthorizeAttribute(params PermissoesDominio[] permissoes)
    {
        AuthenticationSchemes = "Bearer";
        Permissoes = permissoes;
        Policy = "SuperUsuarioOrDominio_" + string.Join('_', permissoes.Select(x => (int)x).Select(x => x.ToString(CultureInfo.InvariantCulture)).ToArray());
    }

    public PermissoesDominio[] Permissoes { get; }
}
