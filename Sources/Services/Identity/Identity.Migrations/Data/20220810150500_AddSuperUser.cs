using DDD.Contracts;
using MongoDB.Bson.Serialization.Attributes;
using Pulsar.BuildingBlocks.DDD;

namespace Pulsar.Services.Identity.Migrations.Data;

[Migration(20220810150500, RequiresTransaction = true)]
public class AddSuperUser : Migration
{
    public override async Task Up()
    {
        var usuarioCollection = GetCollection<Usuario>("Usuarios");
        var sid = Usuario.SuperUsuarioId;
        var salt = GeneralExtensions.GetSalt();
        var superUser = new Usuario(
            sid,
            "Administrador",
            null,
            "administrador",
            salt,
            (salt + "administrador").SHA256Hash(),
            new AuditInfo(sid));
        superUser.IsAtivo = true;
        await usuarioCollection.InsertOneAsync(this.Session, superUser);
    }

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
            DominiosBloqueados = new List<ObjectId>();
            DominiosAdministrados = new List<ObjectId>();
            _termosBusca = GetTermosBusca();
            _nomeCompleto = $"{_primeiroNome} {_ultimoNome}".Trim();
            AuditInfo = auditInfo;
            if (nomeUsuario == "administrador")
                this.IsSuperUsuario = true;
        }

        public string? AvatarUrl { get; set; }
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
    }
}
