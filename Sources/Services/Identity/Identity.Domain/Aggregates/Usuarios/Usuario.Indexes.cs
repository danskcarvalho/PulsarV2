namespace Pulsar.Services.Identity.Domain.Aggregates.Usuarios;

public partial class Usuario
{
    public class Indexes : IndexDescriptions<Usuario>
    {
        public static IX Email_v1 = Describe.Ascending(u => u.Email).Unique();
        public static IX NomeUsuario_v1 = Describe.Ascending(u => u.NomeUsuario).Unique();
        public static IX TermosBusca_Email_v1 = Describe.Text(u => u.TermosBusca);
        public static IX DominiosBloqueados_Email_v1 = Describe.Ascending(u => u.DominiosBloqueados).Ascending(u => u.Email);

        public override string CollectionName => Constants.CollectionNames.USUARIOS;
    }
}
