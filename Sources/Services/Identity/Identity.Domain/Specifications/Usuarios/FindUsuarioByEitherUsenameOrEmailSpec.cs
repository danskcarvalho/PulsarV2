using Pulsar.Services.Identity.Domain.Aggregates.Usuarios;

namespace Pulsar.Services.Identity.Domain.Specifications.Usuarios;

public class FindUsuarioByEitherUsenameOrEmailSpec : IFindSpecification<Usuario>
{
    public string? Username { get; }
    public string? Email { get; }

    public FindUsuarioByEitherUsenameOrEmailSpec(string? username, string? email)
    {
        Username = username;
        Email = email;
    }

    public FindSpecification<Usuario> GetSpec()
    {
        var un = Username?.Trim().ToLowerInvariant();
        var em = Email?.Trim().ToLowerInvariant();
        if (Username != null && Email != null)
            return Find.Where<Usuario>(u => u.Email == em || u.NomeUsuario == un).Limit(1).Build();
        else if (Username != null)
            return Find.Where<Usuario>(u => u.NomeUsuario == un).Limit(1).Build();
        else
            return Find.Where<Usuario>(u => u.Email == em).Limit(1).Build();
    }
}
