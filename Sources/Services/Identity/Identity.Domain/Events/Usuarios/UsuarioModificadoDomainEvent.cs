using Pulsar.Services.Identity.Domain.Aggregates.Usuarios;

namespace Pulsar.Services.Identity.Domain.Events.Usuarios;

public class UsuarioModificadoDomainEvent : INotification
{
    public ObjectId UsuarioId { get; set; }
    public string? PublicAvatarUrl { get; set; }
    public string? PrivateAvatarUrl { get; set; }
    public string PrimeiroNome { get; set; }
    public string? UltimoNome { get; set; }
    public string NomeCompleto { get; set; }
    public bool IsAtivo { get; set; }
    public string NomeUsuario { get; set; }
    public bool IsConvitePendente { get; set; }
    public AuditInfo AuditInfo { get; set; }
    public ChangeEvent Modificacao { get; set; }
    public ChangeDetails DetalhesModificacao { get; set; }

    public UsuarioModificadoDomainEvent(ObjectId usuarioId, string? publicAvatarUrl, string? privateAvatarUrl, string primeiroNome, string? ultimoNome, string nomeCompleto, bool isAtivo, string nomeUsuario, 
        bool isConvitePendente, AuditInfo auditInfo, ChangeEvent modificacao, ChangeDetails detalhesModificacao)
    {
        UsuarioId = usuarioId;
        PublicAvatarUrl = publicAvatarUrl;
        PrivateAvatarUrl = privateAvatarUrl;
        PrimeiroNome = primeiroNome;
        UltimoNome = ultimoNome;
        NomeCompleto = nomeCompleto;
        IsAtivo = isAtivo;
        NomeUsuario = nomeUsuario;
        IsConvitePendente = isConvitePendente;
        AuditInfo = auditInfo;
        Modificacao = modificacao;
        DetalhesModificacao = detalhesModificacao;
    }
}
