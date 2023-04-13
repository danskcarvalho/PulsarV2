using Pulsar.Services.Identity.Domain.Events.Usuarios;

namespace Pulsar.Services.Identity.Domain.Aggregates.Usuarios;

public class Usuario : AggregateRoot
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
        Grupos = new HashSet<UsuarioGrupo>();
        DominiosBloqueados = new List<ObjectId>();
        DominiosAdministrados = new List<ObjectId>();
        _termosBusca = GetTermosBusca();
        _nomeCompleto = $"{_primeiroNome} {_ultimoNome}".Trim();
        AuditInfo = auditInfo;
        if (nomeUsuario == "administrador")
            this.IsSuperUsuario = true;
    }

    public UsuarioAvatar? Avatar { get; set; }
    public string TermosBusca
    {
        get => _termosBusca;
        private set => _termosBusca = value;
    }
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
    public string NomeCompleto
    {
        get => _nomeCompleto;
        private set => _nomeCompleto = value;
    }
    public HashSet<UsuarioGrupo> Grupos { get; private set; }
    public bool IsAtivo { get; set; }
    public bool IsSuperUsuario { get; private set; }
    public List<ObjectId> DominiosBloqueados { get; private set; }
    public List<ObjectId> DominiosAdministrados { get; private set; }
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
    public bool IsConvitePendente { get; set; }
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

    public void GerarTokenMudancaSenha(out long previousVersion)
    {
        if (this.Email is null)
            throw new IdentityDomainException(ExceptionKey.UsuarioSemEmail);

        if (this.TokenMudancaSenha is null || this.TokenMudancaSenhaExpiraEm is null || DateTime.UtcNow > this.TokenMudancaSenhaExpiraEm.Value.AddMinutes(-15))
        {
            this.TokenMudancaSenha = GeneralExtensions.GetSalt();
            this.TokenMudancaSenhaExpiraEm = DateTime.UtcNow.AddMinutes(30);
        }
        this.AddDomainEvent(new TokenMudancaSenhaGeradoDE(this.Id, this.PrimeiroNome, this.Email!, this.TokenMudancaSenha));
        previousVersion = Version++;
    }

    public void RecuperarSenha(string token, string senha, out long previousVersion)
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
        previousVersion = Version++;
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
        this.Version++;
    }

    public void Criar()
    {
        this.AddDomainEvent(new UsuarioModificadoDE(this.Id, this.Avatar?.PublicUrl, this.Avatar?.PrivateUrl, this.PrimeiroNome, this.UltimoNome, this.NomeCompleto, this.IsAtivo, this.NomeUsuario,
            this.IsConvitePendente, this.AuditInfo, ChangeEvent.Created));
    }

    public void MudarMinhaSenha(string senhaAtual, string novaSenha, out long previousVersion)
    {
        previousVersion = Version;
        if (IsConvitePendente)
            throw new IdentityDomainException(ExceptionKey.ConviteNaoAceito);
        if (!TestarSenha(senhaAtual))
            throw new IdentityDomainException(ExceptionKey.SenhaAtualInvalida);
        this.MudarMinhaSenha(novaSenha);
        this.AuditInfo = this.AuditInfo.EditadoPor(this.Id);
        Version++;
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
        Version++;
        this.AuditInfo = this.AuditInfo.EditadoPor(this.Id);
        this.AddDomainEvent(new UsuarioModificadoDE(this.Id, this.Avatar?.PublicUrl, this.Avatar?.PrivateUrl, this.PrimeiroNome, this.UltimoNome, this.NomeCompleto, this.IsAtivo, this.NomeUsuario,
            this.IsConvitePendente, this.AuditInfo, ChangeEvent.Edited));
    }

    public void BloquearOuDesbloquear(ObjectId usuarioLogadoId, bool bloquear)
    {
        if (IsSuperUsuario)
            throw new IdentityDomainException(ExceptionKey.SuperUsuarioNaoPodeSerBloqueado);
        this.IsAtivo = !bloquear;
        Version++;
        this.AuditInfo = this.AuditInfo.EditadoPor(usuarioLogadoId);
        this.AddDomainEvent(new UsuarioModificadoDE(this.Id, this.Avatar?.PublicUrl, this.Avatar?.PrivateUrl, this.PrimeiroNome, this.UltimoNome, this.NomeCompleto, this.IsAtivo, this.NomeUsuario,
            this.IsConvitePendente, this.AuditInfo, ChangeEvent.Edited));
    }

    public void AlterarAvatar(string publiclUrl, string internalUrl)
    {
        this.Avatar = new UsuarioAvatar(publiclUrl, internalUrl);
        this.Version++;
        this.AuditInfo = this.AuditInfo.EditadoPor(this.Id);
        this.AddDomainEvent(new UsuarioModificadoDE(this.Id, this.Avatar?.PublicUrl, this.Avatar?.PrivateUrl, this.PrimeiroNome, this.UltimoNome, this.NomeCompleto, this.IsAtivo, this.NomeUsuario,
            this.IsConvitePendente, this.AuditInfo, ChangeEvent.Edited));
    }

    public void RemoverDominioAdministrado(ObjectId usuarioLogadoId, ObjectId dominioId)
    {
        this.DominiosAdministrados.Remove(dominioId);
        this.Version++;
        this.AuditInfo = this.AuditInfo.EditadoPor(this.Id);
    }

    public void AdicionarDominioAdministrado(ObjectId usuarioLogadoId, ObjectId dominioId)
    {
        if (!this.DominiosAdministrados.Contains(dominioId))
            this.DominiosAdministrados.Add(dominioId);
        this.Version++;
        this.AuditInfo = this.AuditInfo.EditadoPor(this.Id);
    }
}
