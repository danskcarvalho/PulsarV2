using Microsoft.JSInterop;
using Pulsar.Web.Client.Clients.Base;
using Pulsar.Web.Client.Models.Shared;

namespace Pulsar.Web.Client.Clients.Identity.Estabelecimentos
{
	public class EstabelecimentoClient(ClientContext clientContext) : ClientBase(clientContext), IEstabelecimentoClient
    {
        protected override string Section => "estabelecimentos";

        protected override string Service => "identity";

        public Task<IdNomeViewModel> Logado() => Get<IdNomeViewModel>("logado");
    }
}
