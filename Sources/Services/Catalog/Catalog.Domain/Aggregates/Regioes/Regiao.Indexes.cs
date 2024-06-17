namespace Pulsar.Services.Catalog.Domain.Aggregates.Regioes;

public partial class Regiao
{
    public class Indexes : IndexDescriptions<Regiao>
    {
        public static IX Nome_v1 = Describe.Text(x => x.TermosPesquisa);
        public static IX Codigo_v1 = Describe.Ascending(x => x.Codigo);
        public override string CollectionName => Constants.CollectionNames.REGIOES;
    }
}
