using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Pulsar.BuildingBlocks.Sync.Contracts;

namespace Pulsar.Services.Facility.Contracts.Shadows;

[Shadow("Facility:RedesEstabelecimentos")]
public class RedeEstabelecimentosShadow : Shadow
{
    public ObjectId DominioId { get; private set; }
    public string Nome { get; set; }
    public AuditInfoShadow AuditInfo { get; set; }

    [JsonConstructor, BsonConstructor]
    public RedeEstabelecimentosShadow(ObjectId id, ObjectId dominioId, string nome, AuditInfoShadow auditInfo, DateTime timeStamp) : base(id, timeStamp)
    {
        DominioId = dominioId;
        Nome = nome;
        AuditInfo = auditInfo;
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public RedeEstabelecimentosShadow() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}
