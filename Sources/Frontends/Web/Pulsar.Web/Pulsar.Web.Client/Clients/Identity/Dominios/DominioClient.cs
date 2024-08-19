using Microsoft.JSInterop;
using Pulsar.Web.Client.Clients.Identity.Base;
using Pulsar.Web.Client.Models.Shared;

namespace Pulsar.Web.Client.Clients.Identity.Dominios;

public class DominioClient(ClientContext clientContext) : ClientBase(clientContext), IDominioClient
{
    protected override string Section => "dominios";
    protected override string Service => "identity";

    public Task<IdNomeViewModel> Logado() => Get<IdNomeViewModel>("logado");
}
