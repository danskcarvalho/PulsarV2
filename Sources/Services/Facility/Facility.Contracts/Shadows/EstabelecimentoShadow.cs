using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Pulsar.BuildingBlocks.Sync.Contracts;

namespace Pulsar.Services.Facility.Contracts.Shadows;

[Shadow("Facility:Estabelecimentos")]
public partial class EstabelecimentoShadow : Shadow
{
    public ObjectId DominioId { get; private set; }
    public string Nome { get; set; }
    public string Cnes { get; set; }
    public List<ObjectId> Redes { get; private set; }
    public bool IsAtivo { get; set; }
    public AuditInfoDTO AuditInfo { get; set; }

    [JsonConstructor, BsonConstructor]
    public EstabelecimentoShadow(ObjectId id, ObjectId dominioId, string nome, string cnes, List<ObjectId> redes, bool isAtivo, AuditInfoDTO auditInfo, DateTime timeStamp) : base(id, timeStamp)
    {
        DominioId = dominioId;
        Nome = nome;
        Cnes = cnes;
        Redes = redes;
        IsAtivo = isAtivo;
        AuditInfo = auditInfo;
    }

}
