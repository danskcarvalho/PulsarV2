namespace Pulsar.Services.Identity.Domain.Aggregates.Usuarios;

public partial class Usuario
{
    public class Indexes : IndexDescriptions<Usuario>
    {
        public static IX Email = Describe.Ascending(u => u.Email);
        public static IX NomeUsuario = Describe.Ascending(u => u.NomeUsuario);
        public static IX TermosBusca = Describe.Text(u => u.TermosBusca).Ascending(u => u.Email);
        public static IX DominiosBloqueados = Describe.Ascending(u => u.DominiosBloqueados).Ascending(u => u.Email);

        public override string CollectionName => Constants.CollectionNames.USUARIOS;
    }
}
