using MongoDB.Bson;
using Pulsar.BuildingBlocks.Sync.Contracts;

namespace Pulsar.Services.Facility.Contracts.Shadows;

[Shadow("Facility:RedesEstabelecimentos")]
public class RedeEstabelecimentosShadow : Shadow
{
    public ObjectId DominioId { get; private set; }
    public string Nome { get; set; }
    public AuditInfoDTO AuditInfo { get; set; }

    public RedeEstabelecimentosShadow(ObjectId id, ObjectId dominioId, string nome, AuditInfoDTO auditInfo) : base(id)
    {
        DominioId = dominioId;
        Nome = nome;
        AuditInfo = auditInfo;
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public RedeEstabelecimentosShadow() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}
