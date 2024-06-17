namespace Pulsar.Services.Catalog.Domain.Aggregates.Materiais;

public partial class Material
{
    public class Indexes : IndexDescriptions<Material>
    {
        public static IX Nome_v1 = Describe.Text(x => x.TermosPesquisa);
        public override string CollectionName => Constants.CollectionNames.MATERIAIS;
    }
}
