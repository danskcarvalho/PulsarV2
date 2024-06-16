namespace Pulsar.Services.Catalog.Domain.Aggregates.Dentes;

public partial class Dente
{
    public class Indexes : IndexDescriptions<Dente>
    {
        public static IX IX_Nome = Describe.Text(x => x.TermosPesquisa);
        public static IX IX_Codigo = Describe.Ascending(x => x.Codigo);
        public override string CollectionName => Constants.CollectionNames.DENTES;
    }
}
