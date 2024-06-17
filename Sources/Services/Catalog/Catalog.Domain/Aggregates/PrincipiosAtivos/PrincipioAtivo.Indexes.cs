namespace Pulsar.Services.Catalog.Domain.Aggregates.PrincipiosAtivos;

public partial class PrincipioAtivo
{
    public class Indexes : IndexDescriptions<PrincipioAtivo>
    {
        public static IX Nome_v1 = Describe.Text(x => x.TermosPesquisa);
        public static IX Codigo_v1 = Describe.Ascending(x => x.CodigoEsus);
        public override string CollectionName => Constants.CollectionNames.PRINCIPIOSATIVOS;
    }
}
