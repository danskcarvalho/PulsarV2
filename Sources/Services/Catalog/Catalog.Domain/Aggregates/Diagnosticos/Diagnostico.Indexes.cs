namespace Pulsar.Services.Catalog.Domain.Aggregates.Diagnosticos;

public partial class Diagnostico
{
    public class Indexes : IndexDescriptions<Diagnostico>
    {
        public static IX Nome_v1 = Describe.Text(x => x.TermosPesquisa);
        public static IX Codigo_v1 = Describe.Ascending(x => x.Codigo);
        public override string CollectionName => Constants.CollectionNames.DIAGNOSTICOS;
    }
}
