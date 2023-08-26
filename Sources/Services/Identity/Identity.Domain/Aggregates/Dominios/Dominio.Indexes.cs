namespace Pulsar.Services.Identity.Domain.Aggregates.Dominios;

public partial class Dominio
{
    public class Indexes : IndexDescriptions<Dominio>
    {
        public static IX TermosBusca_Nome_Id_v1 = Describe.Text(d => d.TermosBusca).Ascending(d => d.Nome).Ascending(d => d.Id);
        public static IX Nome_Id_v1 = Describe.Ascending(d => d.Nome).Ascending(d => d.Id);

        public override string CollectionName => Constants.CollectionNames.DOMINIOS;
    }
}
