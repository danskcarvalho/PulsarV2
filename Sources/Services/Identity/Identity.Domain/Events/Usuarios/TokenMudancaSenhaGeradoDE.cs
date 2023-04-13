namespace Pulsar.Services.Identity.Domain.Events.Usuarios;

public class TokenMudancaSenhaGeradoDE : INotification
{
    public ObjectId UsuarioId { get; }
    public string Nome { get; }
    public string Email { get; }
    public string Token { get; }

    public TokenMudancaSenhaGeradoDE(ObjectId usuarioId, string nome, string email, string token)
    {
        UsuarioId = usuarioId;
        Nome = nome;
        Email = email;
        Token = token;
    }
}
