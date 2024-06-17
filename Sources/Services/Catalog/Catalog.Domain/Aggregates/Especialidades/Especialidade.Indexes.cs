namespace Pulsar.Services.Catalog.Domain.Aggregates.Especialidades;

public partial class Especialidade
{
    public class Indexes : IndexDescriptions<Especialidade>
    {
        public static IX Nome_v1 = Describe.Text(x => x.TermosPesquisa);
        public static IX Codigo_v1 = Describe.Ascending(x => x.Codigo);
        public override string CollectionName => Constants.CollectionNames.ESPECIALIDADES;
    }
}
