using Pulsar.Web.Client.Models.Shared;

namespace Pulsar.Web.Client.Clients.Identity.Estabelecimentos;

public interface IEstabelecimentoClient
{
    Task<IdNomeViewModel> Logado();
}
