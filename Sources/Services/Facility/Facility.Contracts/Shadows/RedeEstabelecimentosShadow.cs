﻿using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Pulsar.BuildingBlocks.Sync.Contracts;

namespace Pulsar.Services.Facility.Contracts.Shadows;

[Shadow("Facility:RedesEstabelecimentos")]
public class RedeEstabelecimentosShadow : Shadow<RedeEstabelecimentosShadow>
{
    public ObjectId DominioId { get; private set; }
    public string Nome { get; set; }
    public AuditInfoShadow AuditInfo { get; set; }

    [JsonConstructor, BsonConstructor]
    public RedeEstabelecimentosShadow(ObjectId id, ObjectId dominioId, string nome, AuditInfoShadow auditInfo) : base(id)
    {
        DominioId = dominioId;
        Nome = nome;
        AuditInfo = auditInfo;
    }

}
