using MediatR;

namespace Pulsar.Services.Identity.Domain.Events;

public class TokenMudancaSenhaGeradoDomainEvent : INotification
{
    public string UsuarioId { get; }
    public string Nome { get; }
    public string Email { get; }
    public string Token { get; }

    public TokenMudancaSenhaGeradoDomainEvent(string usuarioId, string nome, string email, string token)
    {
        UsuarioId = usuarioId;
        Nome = nome;
        Email = email;
        Token = token;
    }
}
