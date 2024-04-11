using Pulsar.Services.Identity.Domain.Aggregates.Usuarios;

namespace Pulsar.Services.Identity.Domain.Specifications.Usuarios;

public class FindUsuarioByUsenameOrEmailSpec : IFindSpecification<Usuario>
{
    public string UsernameOrEmail { get; }

    public FindUsuarioByUsenameOrEmailSpec(string usernameOrEmail)
    {
        UsernameOrEmail = usernameOrEmail;
    }

    public FindSpecification<Usuario> GetSpec()
    {
        var ue = UsernameOrEmail?.Trim().ToLowerInvariant();
        return Find.Where<Usuario>(u => u.Email == ue || u.NomeUsuario == ue).Limit(1).Build();
    }
}
