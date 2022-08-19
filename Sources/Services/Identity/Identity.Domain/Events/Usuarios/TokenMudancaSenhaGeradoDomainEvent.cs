namespace Pulsar.Services.Identity.Domain.Events.Usuarios;

public class TokenMudancaSenhaGeradoDomainEvent : INotification
{
    public ObjectId UsuarioId { get; }
    public string Nome { get; }
    public string Email { get; }
    public string Token { get; }

    public TokenMudancaSenhaGeradoDomainEvent(ObjectId usuarioId, string nome, string email, string token)
    {
        UsuarioId = usuarioId;
        Nome = nome;
        Email = email;
        Token = token;
    }
}
