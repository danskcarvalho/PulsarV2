using DDD.Contracts;
using Pulsar.Services.Identity.Domain.Aggregates.Usuarios;
using Pulsar.Services.Identity.Domain.Events.Convites;

namespace Pulsar.Services.Identity.Domain.Aggregates.Convites;

public class Convite : AggregateRootWithContext<Convite>
{
    [BsonConstructor(nameof(Id), nameof(Email), nameof(ConviteExpiraEm), nameof(TokenAceitacao), nameof(UsuarioId), nameof(AuditInfo))]
    public Convite(ObjectId id, string email, DateTime conviteExpiraEm, string tokenAceitacao, ObjectId usuarioId, AuditInfo auditInfo) : base(id)
    {
        _email = email.Trim().ToLowerInvariant();
        ConviteExpiraEm = conviteExpiraEm;
        TokenAceitacao = tokenAceitacao;
        UsuarioId = usuarioId;
        AuditInfo = auditInfo;
    }

    private string _email;
    public string Email { get => _email; private set => _email = value.Trim().ToLowerInvariant(); }
    public DateTime ConviteExpiraEm { get; private set; }
    public string TokenAceitacao { get; private set; }
    public bool IsAceito { get; set; }
    public ObjectId UsuarioId { get; private set; }
    public Task<Usuario> GetUsuario() => Usuario.GetAndCache(this.UsuarioId, nameof(UsuarioId));
    public AuditInfo AuditInfo { get; set; }

    public void Aceitar(string? primeiroNome, string? sobrenome, string? nomeUsuario, string? senha, string? token)
    {
        if (IsAceito)
            throw new IdentityDomainException(IdentityExceptionKey.ConviteJaAceito);
        if (DateTime.UtcNow > ConviteExpiraEm)
            throw new IdentityDomainException(IdentityExceptionKey.ConviteExpirado);
        if (TokenAceitacao != token)
            throw new IdentityDomainException(IdentityExceptionKey.ConviteTokenInvalido);

        IsAceito = true;
        this.AddDomainEvent(new ConviteAceitoDE(this.Id, this.UsuarioId, primeiroNome!, sobrenome, nomeUsuario!, senha!, this.Email, AuditInfo.CriadoPorUsuarioId));
    }

    public void ConvidarUsuario()
    {
        AddDomainEvent(new UsuarioConvidadoDE(this.Id, this.UsuarioId, this.Email, this.TokenAceitacao, this.AuditInfo.CriadoPorUsuarioId!.Value));
    }
}
