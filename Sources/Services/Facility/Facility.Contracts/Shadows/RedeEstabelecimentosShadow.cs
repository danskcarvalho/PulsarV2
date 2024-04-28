using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Pulsar.BuildingBlocks.Sync.Contracts;

namespace Pulsar.Services.Facility.Contracts.Shadows;

[Shadow("Facility:RedesEstabelecimentos")]
public class RedeEstabelecimentosShadow : Shadow
{
    public ObjectId DominioId { get; private set; }
    public string Nome { get; set; }
    public AuditInfoDTO AuditInfo { get; set; }

    [JsonConstructor, BsonConstructor]
    public RedeEstabelecimentosShadow(ObjectId id, ObjectId dominioId, string nome, AuditInfoDTO auditInfo, DateTime timeStamp) : base(id, timeStamp)
    {
        DominioId = dominioId;
        Nome = nome;
        AuditInfo = auditInfo;
    }
}
