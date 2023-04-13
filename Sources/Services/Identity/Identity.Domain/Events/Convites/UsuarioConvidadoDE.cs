namespace Pulsar.Services.Identity.Domain.Events.Convites;

public class UsuarioConvidadoDE : INotification
{
    public UsuarioConvidadoDE(ObjectId conviteId, ObjectId usuarioId, string email, string tokenAceitacao, ObjectId usuarioLogadoId)
    {
        ConviteId = conviteId;
        UsuarioId = usuarioId;
        Email = email;
        TokenAceitacao = tokenAceitacao;
        UsuarioLogadoId = usuarioLogadoId;
    }

    public ObjectId ConviteId { get; }
    public ObjectId UsuarioId { get; }
    public string Email { get; }
    public string TokenAceitacao { get; }
    public ObjectId UsuarioLogadoId { get; }
}
