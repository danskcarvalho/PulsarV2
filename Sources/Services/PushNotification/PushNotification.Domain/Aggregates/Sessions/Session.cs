using DDD.Contracts;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Pulsar.BuildingBlocks.DDD;
using Pulsar.Services.PushNotification.Domain.Events.PushNotifications;

namespace Pulsar.Services.PushNotification.Domain.Aggregates.Sessions;

public partial class Session : AggregateRoot
{
	public static readonly TimeSpan SESSION_DURATION = TimeSpan.FromMinutes(5);

	[BsonConstructor]
	public Session(ObjectId id, ObjectId usuarioId, ObjectId? dominioId, ObjectId? estabelecimentoId, string token) : base(id)
	{
		UsuarioId = usuarioId;
		DominioId = dominioId;
		EstabelecimentoId = estabelecimentoId;
		CreatedOn = DateTime.UtcNow;
		ExpiresOn = CreatedOn.Add(SESSION_DURATION);
		Token = token;
	}

	public ObjectId UsuarioId { get; private set; }
	public ObjectId? DominioId { get; private set; }
	public ObjectId? EstabelecimentoId { get; private set; }
	public string Token { get; private set; }
	public DateTime ExpiresOn { get; private set; }
	public DateTime CreatedOn { get; private set; }

	public void Criar()
	{
		this.AddDomainEvent(new SessaoCriadaDE(this.Id, this.UsuarioId, this.DominioId, this.EstabelecimentoId));
	}
}
