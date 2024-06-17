namespace Pulsar.Services.Catalog.Domain.Aggregates.Dentes;

public partial class Dente
{
    public class Indexes : IndexDescriptions<Dente>
    {
        public static IX Nome_v1 = Describe.Text(x => x.TermosPesquisa);
        public static IX Codigo_v1 = Describe.Ascending(x => x.Codigo);
        public override string CollectionName => Constants.CollectionNames.DENTES;
    }
}
