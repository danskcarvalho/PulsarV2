using Microsoft.AspNetCore.Authorization;
using System.Globalization;

namespace Pulsar.Services.Identity.API.Authorization;

public class DominioAuthorizeAttribute : AuthorizeAttribute
{
    public DominioAuthorizeAttribute(params PermissoesDominio[] permissoes)
    {
        Permissoes = permissoes;
        Policy = "Dominio_" + string.Join('_', permissoes.Select(x => (int)x).Select(x => x.ToString(CultureInfo.InvariantCulture)).ToArray());
    }

    public PermissoesDominio[] Permissoes { get; }
}
