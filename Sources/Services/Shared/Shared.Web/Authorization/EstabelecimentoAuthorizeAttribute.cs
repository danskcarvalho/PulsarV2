using Microsoft.AspNetCore.Authorization;
using Pulsar.Services.Identity.Contracts.Enumerations;
using System.Globalization;

namespace Pulsar.Services.Shared.API.Authorization;

public class EstabelecimentoAuthorizeAttribute : AuthorizeAttribute
{
    public EstabelecimentoAuthorizeAttribute(params PermissoesEstabelecimento[] permissoes)
    {
        Permissoes = permissoes;
        Policy = "Estabelecimento_" + string.Join('_', permissoes.Select(x => (int)x).Select(x => x.ToString(CultureInfo.InvariantCulture)).ToArray());
    }

    public PermissoesEstabelecimento[] Permissoes { get; }
}
