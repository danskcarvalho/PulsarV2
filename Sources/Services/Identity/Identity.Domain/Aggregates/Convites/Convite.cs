using Pulsar.Services.Identity.Domain.Events.Convites;

namespace Pulsar.Services.Identity.Domain.Aggregates.Convites;

public class Convite : AggregateRoot
{
    [BsonConstructor(nameof(Id), nameof(Email), nameof(ConviteExpiraEm), nameof(TokenAceitacao), nameof(DominioId), nameof(AdministrarDominio), nameof(Grupos), nameof(AuditInfo))]
    public Convite(ObjectId id, string email, DateTime conviteExpiraEm, string tokenAceitacao, ObjectId dominioId, bool administrarDominio, IEnumerable<ConviteGrupo> grupos, AuditInfo auditInfo) : base(id)
    {
        _email = email.Trim().ToLowerInvariant();
        ConviteExpiraEm = conviteExpiraEm;
        TokenAceitacao = tokenAceitacao;
        DominioId = dominioId;
        AdministrarDominio = administrarDominio;
        Grupos = new HashSet<ConviteGrupo>(grupos);
        AuditInfo = auditInfo;
    }

    private string _email;
    public string Email { get => _email; private set => _email = value.Trim().ToLowerInvariant(); }
    public DateTime ConviteExpiraEm { get; private set; }
    public string TokenAceitacao { get; private set; }
    public bool IsAceito { get; set; }
    public ObjectId DominioId { get; private set; }
    /// <summary>
    /// Caso o domínio já esteja sendo administrado, o convite falha.
    /// </summary>
    public bool AdministrarDominio { get; private set; }
    public IReadOnlySet<ConviteGrupo> Grupos { get; private set; }

    public AuditInfo AuditInfo { get; set; }

    public void Aceitar(string? primeiroNome, string? sobrenome, string? nomeUsuario, string? senha, string? token)
    {
        if (IsAceito)
            throw new IdentityDomainException(ExceptionKey.ConviteJaAceito);
        if (DateTime.UtcNow > ConviteExpiraEm)
            throw new IdentityDomainException(ExceptionKey.ConviteExpirado);
        if (TokenAceitacao != token)
            throw new IdentityDomainException(ExceptionKey.ConviteTokenInvalido);

        IsAceito = true;
        Version++;
        this.AddDomainEvent(new ConviteAceitoDomainEvent(DominioId, this.Id, ObjectId.GenerateNewId(), primeiroNome!, sobrenome, nomeUsuario!, senha!, this.Email, this.AdministrarDominio, this.Grupos, AuditInfo.CriadoPorUsuarioId));

    }
}
