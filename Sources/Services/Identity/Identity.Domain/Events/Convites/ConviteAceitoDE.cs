using Pulsar.Services.Identity.Domain.Aggregates.Convites;

namespace Pulsar.Services.Identity.Domain.Events.Convites;

public class ConviteAceitoDE : INotification
{
    public ObjectId ConviteId { get; set; }
    public ObjectId UsuarioId { get; set; }
    public string PrimeiroNome { get; set; }
    public string? Sobrenome { get; set; }
    public string NomeUsuario { get; set; }
    public string Senha { get; set; }
    public string Email { get; set; }
    public ObjectId? CriadoPorUsuarioId { get; }

    public ConviteAceitoDE(ObjectId conviteId, ObjectId usuarioId, string primeiroNome, string? sobrenome, string nomeUsuario, string senha, string email, ObjectId? criadoPorUsuarioId)
    {
        ConviteId = conviteId;
        UsuarioId = usuarioId;
        PrimeiroNome = primeiroNome;
        Sobrenome = sobrenome;
        NomeUsuario = nomeUsuario;
        Senha = senha;
        Email = email;
        CriadoPorUsuarioId = criadoPorUsuarioId;
    }
}
