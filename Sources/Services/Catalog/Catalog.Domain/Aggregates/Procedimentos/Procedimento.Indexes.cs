namespace Pulsar.Services.Catalog.Domain.Aggregates.Procedimentos;

public partial class Procedimento
{
    public class Indexes : IndexDescriptions<Procedimento>
    {
        public static IX Nome_v1 = Describe.Text(x => x.TermosPesquisa);
        public static IX Codigo_v1 = Describe.Ascending(x => x.Codigo);
        public override string CollectionName => Constants.CollectionNames.PROCEDIMENTOS;
    }
}
