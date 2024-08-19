using Pulsar.Web.Client.Models.Shared;

namespace Pulsar.Web.Client.Clients.Identity.Dominios;

public interface IDominioClient
{
    Task<IdNomeViewModel> Logado();
}
