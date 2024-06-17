namespace Pulsar.Services.Catalog.Domain.Aggregates.Ineps;

public partial class Inep
{
    public class Indexes : IndexDescriptions<Inep>
    {
        public static IX Nome_v1 = Describe.Text(x => x.TermosPesquisa);
        public static IX Codigo_v1 = Describe.Ascending(x => x.Codigo);
        public override string CollectionName => Constants.CollectionNames.INEPS;
    }
}
