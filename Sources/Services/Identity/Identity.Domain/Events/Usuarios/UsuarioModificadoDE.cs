using Pulsar.Services.Identity.Domain.Aggregates.Usuarios;

namespace Pulsar.Services.Identity.Domain.Events.Usuarios;

public class UsuarioModificadoDE : INotification
{
    public ObjectId UsuarioId { get; set; }
    public string? AvatarUrl { get; set; }
    public string PrimeiroNome { get; set; }
    public string? UltimoNome { get; set; }
    public string NomeCompleto { get; set; }
    public bool IsAtivo { get; set; }
    public string NomeUsuario { get; set; }
    public string? Email { get; set; }
    public bool IsConvitePendente { get; set; }
    public AuditInfo AuditInfo { get; set; }
    public ChangeEvent Modificacao { get; set; }

    public UsuarioModificadoDE(ObjectId usuarioId, string? avatarUrl, string primeiroNome, string? ultimoNome, string nomeCompleto, bool isAtivo, string nomeUsuario, string? email,
        bool isConvitePendente, AuditInfo auditInfo, ChangeEvent modificacao)
    {
        Email = email;
        UsuarioId = usuarioId;
        AvatarUrl = avatarUrl;
        PrimeiroNome = primeiroNome;
        UltimoNome = ultimoNome;
        NomeCompleto = nomeCompleto;
        IsAtivo = isAtivo;
        NomeUsuario = nomeUsuario;
        IsConvitePendente = isConvitePendente;
        AuditInfo = auditInfo;
        Modificacao = modificacao;
    }
}
