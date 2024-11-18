using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Pulsar.BuildingBlocks.Sync.Contracts;
using Pulsar.Services.Shared.DTOs;

namespace Pulsar.Services.Identity.Contracts.Shadows;

[Shadow("Identity:Usuarios")]
public class UsuarioShadow : Shadow
{
    public string? AvatarUrl { get; private set; }
    public string PrimeiroNome { get; private set; }
    public string? UltimoNome { get; private set; }
    public string NomeCompleto { get; private set; }
    public bool IsAtivo { get; set; }
    public bool IsSuperUsuario { get; private set; }
    public string NomeUsuario { get; private set; }
    public bool IsConvitePendente { get; private set; }
    public AuditInfoShadow AuditInfo { get; private set; }
    public string? Email { get; private set; }

    [JsonConstructor, BsonConstructor]
    public UsuarioShadow(
        ObjectId id,
        string? avatarUrl,
        string primeiroNome,
        string? ultimoNome,
        string nomeCompleto,
        bool isAtivo,
        bool isSuperUsuario,
        string nomeUsuario,
        bool isConvitePendente,
        AuditInfoShadow auditInfo,
        string? email,
        DateTime timeStamp) : base(id, timeStamp)
    {
        AvatarUrl = avatarUrl;
        PrimeiroNome = primeiroNome;
        UltimoNome = ultimoNome;
        NomeCompleto = nomeCompleto;
        IsAtivo = isAtivo;
        IsSuperUsuario = isSuperUsuario;
        NomeUsuario = nomeUsuario;
        IsConvitePendente = isConvitePendente;
        AuditInfo = auditInfo;
        Email = email;
    }
}
