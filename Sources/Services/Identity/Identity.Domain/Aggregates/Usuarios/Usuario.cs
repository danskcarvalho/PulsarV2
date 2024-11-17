using Pulsar.BuildingBlocks.DDD;
using Pulsar.BuildingBlocks.Sync.Contracts;
using Pulsar.Services.Identity.Contracts.Shadows;
using Pulsar.Services.Identity.Domain.Events.Usuarios;

namespace Pulsar.Services.Identity.Domain.Aggregates.Usuarios;

[TrackChanges(CollectionName = Constants.CollectionNames.USUARIOS, ShadowType = typeof(UsuarioShadow))]
public partial class Usuario : AggregateRootWithContext<Usuario>
{
    public static readonly ObjectId SuperUsuarioId = ObjectId.Parse("62f3f4201dbf5877ae6fe940");

    private string _primeiroNome;
    private string? _ultimoNome;
    private string _nomeCompleto;
    private string? _email;
    private string _nomeUsuario;
    private string _termosBusca;

    [BsonConstructor]
    public Usuario(ObjectId id, string primeiroNome, string? email, string nomeUsuario, string senhaSalt, string senhaHash, AuditInfo auditInfo) : base(id)
    {
        _primeiroNome = primeiroNome;
        _email = email?.Trim().ToLowerInvariant();
        _nomeUsuario = nomeUsuario;
        SenhaSalt = senhaSalt;
        SenhaHash = senhaHash;
        DominiosBloqueados = new List<ObjectId>();
        DominiosAdministrados = new List<ObjectId>();
        _termosBusca = GetTermosBusca();
        _nomeCompleto = $"{_primeiroNome} {_ultimoNome}".Trim();
        AuditInfo = auditInfo;
        if (nomeUsuario == "administrador")
            this.IsSuperUsuario = true;
    }
    
    [TrackChanges]
    public string? AvatarUrl { get; set; }

    public string TermosBusca
    {
        get => _termosBusca;
        private set => _termosBusca = value;
    }

    [TrackChanges]
    public string PrimeiroNome
    {
        get => _primeiroNome;
        set
        {
            _primeiroNome = value;
            if (!IsInitializing)
            {
                UpdateNomeCompleto();
                _termosBusca = GetTermosBusca();
            }
        }
    }

    [TrackChanges]
    public string? UltimoNome
    {
        get => _ultimoNome;
        set
        {
            _ultimoNome = value;
            if (!IsInitializing)
            {
                UpdateNomeCompleto();
                _termosBusca = GetTermosBusca();
            }
        }
    }

    [TrackChanges]
    public string NomeCompleto
    {
        get => _nomeCompleto;
        private set => _nomeCompleto = value;
    }
    
    [TrackChanges]
    public bool IsAtivo { get; set; }

    [TrackChanges]
    public bool IsSuperUsuario { get; private set; }
    public List<ObjectId> DominiosBloqueados { get; private set; }
    public List<ObjectId> DominiosAdministrados { get; private set; }

    [TrackChanges]
    public string? Email
    {
        get => _email;
        private set
        {
            _email = value?.Trim().ToLowerInvariant();
            if (!IsInitializing)
                _termosBusca = GetTermosBusca();
        }
    }

    [TrackChanges]
    public string NomeUsuario
    {
        get => _nomeUsuario;
        private set
        {
            _nomeUsuario = value.Trim().ToLowerInvariant();
            if (!IsInitializing)
                _termosBusca = GetTermosBusca();
        }
    }
    public string SenhaHash { get; set; }
    public string SenhaSalt { get; set; }
    public string? TokenMudancaSenha { get; set; }
    public DateTime? TokenMudancaSenhaExpiraEm { get; set; }

    [TrackChanges]
    public bool IsConvitePendente { get; set; }

    [TrackChanges]
    public AuditInfo AuditInfo { get; set; }

    private void UpdateNomeCompleto()
    {
        _nomeCompleto = $"{_primeiroNome} {_ultimoNome}".Trim();
    }
    private string GetTermosBusca()
    {
        return $"{_primeiroNome};{_ultimoNome};{_email};{_nomeUsuario}".Tokenize()!;
    }

    public bool TestarSenha(string password)
    {
        return (this.SenhaSalt + password).SHA256Hash() == this.SenhaHash;
    }

    public void GerarTokenMudancaSenha()
    {
        if (this.Email is null)
            throw new IdentityDomainException(IdentityExceptionKey.UsuarioSemEmail);

        if (this.TokenMudancaSenha is null || this.TokenMudancaSenhaExpiraEm is null || DateTime.UtcNow > this.TokenMudancaSenhaExpiraEm.Value.AddMinutes(-15))
        {
            this.TokenMudancaSenha = GeneralExtensions.GetSalt();
            this.TokenMudancaSenhaExpiraEm = DateTime.UtcNow.AddMinutes(30);
        }
        this.AddDomainEvent(new TokenMudancaSenhaGeradoDE(this.Id, this.PrimeiroNome, this.Email!, this.TokenMudancaSenha));
    }

    public void RecuperarSenha(string token, string senha)
    {
        if (TokenMudancaSenhaExpiraEm == null)
            throw new IdentityDomainException(IdentityExceptionKey.TokenMudancaSenhaExpirado);
        if (DateTime.UtcNow > this.TokenMudancaSenhaExpiraEm)
            throw new IdentityDomainException(IdentityExceptionKey.TokenMudancaSenhaExpirado);
        if (TokenMudancaSenha != token)
            throw new IdentityDomainException(IdentityExceptionKey.TokenMudancaSenhaInvalido);

        this.SenhaSalt = GeneralExtensions.GetSalt();
        this.SenhaHash = (this.SenhaSalt + senha).SHA256Hash();
        this.TokenMudancaSenha = null;
        this.TokenMudancaSenhaExpiraEm = null;
    }

    public void AceitarConvite(string primeiroNome, string? sobrenome, string nomeUsuario, string senha)
    {
        this.PrimeiroNome = primeiroNome;
        this.UltimoNome = sobrenome;
        this.NomeUsuario = nomeUsuario;
        this.SenhaSalt = GeneralExtensions.GetSalt();
        this.SenhaHash = (this.SenhaSalt + senha).SHA256Hash();
        this.IsConvitePendente = false;
        this.IsAtivo = true;
    }

    public void Criar()
    {
        this.AddDomainEvent(new UsuarioModificadoDE(this.Id, this.AvatarUrl, this.PrimeiroNome, this.UltimoNome, this.NomeCompleto, this.IsAtivo, this.NomeUsuario, this.Email,
            this.IsConvitePendente, this.AuditInfo, ChangeEvent.Created));
    }

    public void MudarMinhaSenha(string senhaAtual, string novaSenha)
    {
        var previousVersion = Version;
        if (IsConvitePendente)
            throw new IdentityDomainException(IdentityExceptionKey.ConviteNaoAceito);
        if (!TestarSenha(senhaAtual))
            throw new IdentityDomainException(IdentityExceptionKey.SenhaAtualInvalida);
        this.MudarMinhaSenha(novaSenha);
        this.AuditInfo = this.AuditInfo.EditadoPor(this.Id);
    }

    private void MudarMinhaSenha(string novaSenha)
    {
        this.SenhaSalt = GeneralExtensions.GetSalt();
        this.SenhaHash = (this.SenhaSalt + novaSenha).SHA256Hash();
    }

    public void EditarMeusDados(string primeiroNome, string? sobrenome)
    {
        this.PrimeiroNome = primeiroNome;
        this.UltimoNome = sobrenome;
        this.AuditInfo = this.AuditInfo.EditadoPor(this.Id);
        this.AddDomainEvent(new UsuarioModificadoDE(this.Id, this.AvatarUrl, this.PrimeiroNome, this.UltimoNome, this.NomeCompleto, this.IsAtivo, this.NomeUsuario, this.Email,
            this.IsConvitePendente, this.AuditInfo, ChangeEvent.Edited));
    }

    public void BloquearOuDesbloquear(ObjectId usuarioLogadoId, bool bloquear)
    {
        if (IsSuperUsuario)
            throw new IdentityDomainException(IdentityExceptionKey.SuperUsuarioNaoPodeSerBloqueado);
        this.IsAtivo = !bloquear;
        this.AuditInfo = this.AuditInfo.EditadoPor(usuarioLogadoId);
        this.AddDomainEvent(new UsuarioModificadoDE(this.Id, this.AvatarUrl, this.PrimeiroNome, this.UltimoNome, this.NomeCompleto, this.IsAtivo, this.NomeUsuario, this.Email,
            this.IsConvitePendente, this.AuditInfo, ChangeEvent.Edited));
    }

    public void AlterarAvatar(string url)
    {
        this.AvatarUrl = url;
        this.AuditInfo = this.AuditInfo.EditadoPor(this.Id);
        this.AddDomainEvent(new UsuarioModificadoDE(this.Id, this.AvatarUrl, this.PrimeiroNome, this.UltimoNome, this.NomeCompleto, this.IsAtivo, this.NomeUsuario, this.Email,
            this.IsConvitePendente, this.AuditInfo, ChangeEvent.Edited));
    }

    public void RemoverDominioAdministrado(ObjectId usuarioLogadoId, ObjectId dominioId)
    {
        this.DominiosAdministrados.Remove(dominioId);
        this.AuditInfo = this.AuditInfo.EditadoPor(this.Id);
    }

    public void AdicionarDominioAdministrado(ObjectId usuarioLogadoId, ObjectId dominioId)
    {
        if (!this.DominiosAdministrados.Contains(dominioId))
            this.DominiosAdministrados.Add(dominioId);
        this.AuditInfo = this.AuditInfo.EditadoPor(this.Id);
    }
}
