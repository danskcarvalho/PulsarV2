namespace Pulsar.Services.Catalog.Domain.Aggregates.Etnias;

public partial class Etnia
{
    public class Indexes : IndexDescriptions<Etnia>
    {
        public static IX Nome_v1 = Describe.Text(x => x.TermosPesquisa);
        public static IX Codigo_v1 = Describe.Ascending(x => x.Codigo);
        public override string CollectionName => Constants.CollectionNames.ETNIAS;
    }
}
