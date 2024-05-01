using Pulsar.BuildingBlocks.DDD;
using Pulsar.BuildingBlocks.Sync.Contracts;
using Pulsar.Services.Identity.Contracts.Shadows;
using Pulsar.Services.Identity.Domain.Events.Usuarios;

namespace Pulsar.Services.Identity.Domain.Aggregates.Usuarios;

[TrackChanges(CollectionName = Constants.CollectionNames.USUARIOS, ShadowType = typeof(UsuarioShadow))]
public partial class Usuario : AggregateRootWithCrud<Usuario>
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

    public async Task GerarTokenMudancaSenha()
    {
        if (this.Email is null)
            throw new IdentityDomainException(ExceptionKey.UsuarioSemEmail);

        if (this.TokenMudancaSenha is null || this.TokenMudancaSenhaExpiraEm is null || DateTime.UtcNow > this.TokenMudancaSenhaExpiraEm.Value.AddMinutes(-15))
        {
            this.TokenMudancaSenha = GeneralExtensions.GetSalt();
            this.TokenMudancaSenhaExpiraEm = DateTime.UtcNow.AddMinutes(30);
        }
        this.AddDomainEvent(new TokenMudancaSenhaGeradoDE(this.Id, this.PrimeiroNome, this.Email!, this.TokenMudancaSenha));
        var previousVersion = Version++;
        await this.Replace(previousVersion).CheckModified();
    }

    public async Task RecuperarSenha(string token, string senha)
    {
        if (TokenMudancaSenhaExpiraEm == null)
            throw new IdentityDomainException(ExceptionKey.TokenMudancaSenhaExpirado);
        if (DateTime.UtcNow > this.TokenMudancaSenhaExpiraEm)
            throw new IdentityDomainException(ExceptionKey.TokenMudancaSenhaExpirado);
        if (TokenMudancaSenha != token)
            throw new IdentityDomainException(ExceptionKey.TokenMudancaSenhaInvalido);

        this.SenhaSalt = GeneralExtensions.GetSalt();
        this.SenhaHash = (this.SenhaSalt + senha).SHA256Hash();
        this.TokenMudancaSenha = null;
        this.TokenMudancaSenhaExpiraEm = null;
        var previousVersion = Version++;
        await this.Replace(previousVersion).CheckModified();
    }

    public async Task AceitarConvite(string primeiroNome, string? sobrenome, string nomeUsuario, string senha)
    {
        this.PrimeiroNome = primeiroNome;
        this.UltimoNome = sobrenome;
        this.NomeUsuario = nomeUsuario;
        this.SenhaSalt = GeneralExtensions.GetSalt();
        this.SenhaHash = (this.SenhaSalt + senha).SHA256Hash();
        this.IsConvitePendente = false;
        this.IsAtivo = true;
        this.Version++;
        await this.Replace();
    }

    public async Task Criar()
    {
        this.AddDomainEvent(new UsuarioModificadoDE(this.Id, this.AvatarUrl, this.PrimeiroNome, this.UltimoNome, this.NomeCompleto, this.IsAtivo, this.NomeUsuario, this.Email,
            this.IsConvitePendente, this.AuditInfo, ChangeEvent.Created));
        await this.Insert();
    }

    public async Task MudarMinhaSenha(string senhaAtual, string novaSenha)
    {
        var previousVersion = Version;
        if (IsConvitePendente)
            throw new IdentityDomainException(ExceptionKey.ConviteNaoAceito);
        if (!TestarSenha(senhaAtual))
            throw new IdentityDomainException(ExceptionKey.SenhaAtualInvalida);
        this.MudarMinhaSenha(novaSenha);
        this.AuditInfo = this.AuditInfo.EditadoPor(this.Id);
        Version++;
        await this.Replace(previousVersion).CheckModified();
    }

    private void MudarMinhaSenha(string novaSenha)
    {
        this.SenhaSalt = GeneralExtensions.GetSalt();
        this.SenhaHash = (this.SenhaSalt + novaSenha).SHA256Hash();
    }

    public async Task EditarMeusDados(string primeiroNome, string? sobrenome)
    {
        this.PrimeiroNome = primeiroNome;
        this.UltimoNome = sobrenome;
        Version++;
        this.AuditInfo = this.AuditInfo.EditadoPor(this.Id);
        this.AddDomainEvent(new UsuarioModificadoDE(this.Id, this.AvatarUrl, this.PrimeiroNome, this.UltimoNome, this.NomeCompleto, this.IsAtivo, this.NomeUsuario, this.Email,
            this.IsConvitePendente, this.AuditInfo, ChangeEvent.Edited));
        await this.Replace();
    }

    public async Task BloquearOuDesbloquear(ObjectId usuarioLogadoId, bool bloquear)
    {
        if (IsSuperUsuario)
            throw new IdentityDomainException(ExceptionKey.SuperUsuarioNaoPodeSerBloqueado);
        this.IsAtivo = !bloquear;
        Version++;
        this.AuditInfo = this.AuditInfo.EditadoPor(usuarioLogadoId);
        this.AddDomainEvent(new UsuarioModificadoDE(this.Id, this.AvatarUrl, this.PrimeiroNome, this.UltimoNome, this.NomeCompleto, this.IsAtivo, this.NomeUsuario, this.Email,
            this.IsConvitePendente, this.AuditInfo, ChangeEvent.Edited));
        await this.Replace();
    }

    public async Task AlterarAvatar(string url)
    {
        this.AvatarUrl = url;
        this.Version++;
        this.AuditInfo = this.AuditInfo.EditadoPor(this.Id);
        this.AddDomainEvent(new UsuarioModificadoDE(this.Id, this.AvatarUrl, this.PrimeiroNome, this.UltimoNome, this.NomeCompleto, this.IsAtivo, this.NomeUsuario, this.Email,
            this.IsConvitePendente, this.AuditInfo, ChangeEvent.Edited));
        await this.Replace();
    }

    public async Task RemoverDominioAdministrado(ObjectId usuarioLogadoId, ObjectId dominioId)
    {
        this.DominiosAdministrados.Remove(dominioId);
        this.Version++;
        this.AuditInfo = this.AuditInfo.EditadoPor(this.Id);
        await this.Replace();
    }

    public async Task AdicionarDominioAdministrado(ObjectId usuarioLogadoId, ObjectId dominioId)
    {
        if (!this.DominiosAdministrados.Contains(dominioId))
            this.DominiosAdministrados.Add(dominioId);
        this.Version++;
        this.AuditInfo = this.AuditInfo.EditadoPor(this.Id);
        await this.Replace();
    }
}
