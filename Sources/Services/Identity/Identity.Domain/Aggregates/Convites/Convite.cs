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
    public ObjectId DominioId { get; private set; }
    /// <summary>
    /// Caso o domínio já esteja sendo administrado, o convite falha.
    /// </summary>
    public bool AdministrarDominio { get; private set; }
    public IReadOnlySet<ConviteGrupo> Grupos { get; private set; }

    public AuditInfo AuditInfo { get; set; }
}
