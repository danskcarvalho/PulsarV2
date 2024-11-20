using Pulsar.BuildingBlocks.Sync.Contracts;
using Pulsar.Services.Shared.DTOs;

namespace Pulsar.Services.PushNotification.Migrations.Data;

[Migration(20220810150500, RequiresTransaction = true)]
public class AddSuperUser : Migration
{
	public override async Task Up()
	{
		var usuarioCollection = GetCollection<UsuarioShadow>("_IdentityUsuarios_Shadow");
		var sid = UsuarioShadow.SuperUsuarioId;
		var salt = GeneralExtensions.GetSalt();
		var superUser = new UsuarioShadow(
			sid,
			null,
			"Administrador",
			null,
			"Administrador",
			true,
			true,
			"administrador",
			false,
			new AuditInfoShadow() { CriadoEm = DateTime.UtcNow, CriadoPorUsuarioId = sid },
			null,
			DateTime.UtcNow);
		superUser.IsAtivo = true;
		await usuarioCollection.InsertOneAsync(Session, superUser);
	}
	public class UsuarioShadow
	{
		public static readonly ObjectId SuperUsuarioId = ObjectId.Parse("62f3f4201dbf5877ae6fe940");
		public ObjectId Id { get; private set; }
		public long Version { get; private set; }
		public DateTime TimeStamp { get; private set; }
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
			DateTime timeStamp)
		{
			Id = id;
			TimeStamp = timeStamp;
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
}
