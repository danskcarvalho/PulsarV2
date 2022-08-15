using Pulsar.Services.Identity.Domain.Aggregates.Usuarios;

namespace Pulsar.Services.Identity.Domain.Specifications;

public class GetUsuarioByUsenameOrEmailSpec : IFindSpecification<Usuario>
{
    public string UsernameOrEmail { get; }

    public GetUsuarioByUsenameOrEmailSpec(string usernameOrEmail)
    {
        UsernameOrEmail = usernameOrEmail;
    }

    public FindSpecification<Usuario> GetSpec()
    {
        var ue = UsernameOrEmail?.Trim().ToLowerInvariant();
        return Find.Where<Usuario>(u => u.Email == ue || u.NomeUsuario == ue).Limit(1).Build();
    }
}
