using Pulsar.BuildingBlocks.DDD;

namespace Pulsar.Services.PushNotification.Domain.Aggregates.UserContexts;

public partial class UserContext
{
	public class Indexes : IndexDescriptions<UserContext>
	{
		public static IX Usuario_Dominio_Estabelecimento_v1 = Describe.Ascending(x => x.UsuarioId).Ascending(x => x.DominioId).Ascending(x => x.EstabelecimentoId).Unique();
		public static IX Usuario_Estabelecimento_v1 = Describe.Ascending(x => x.UsuarioId).Ascending(x => x.EstabelecimentoId).Unique();
		public override string CollectionName => Constants.CollectionNames.USER_CONTEXTS;
	}

}
