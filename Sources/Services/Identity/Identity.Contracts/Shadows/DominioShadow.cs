using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Pulsar.BuildingBlocks.Sync.Contracts;
using Pulsar.Services.Shared.DTOs;

namespace Pulsar.Services.Identity.Contracts.Shadows;

[Shadow("Identity:Dominios")]
public class DominioShadow : Shadow<DominioShadow>
{
    public string Nome { get; private set; }
    public bool IsAtivo { get; set; }
    public AuditInfoShadow AuditInfo { get; set; }
    public ObjectId? UsuarioAdministradorId { get; set; }

    [JsonConstructor, BsonConstructor]
    public DominioShadow(ObjectId id, string nome, bool isAtivo, AuditInfoShadow auditInfo, ObjectId? usuarioAdministradorId) : base(id)
    {
        Nome = nome;
        IsAtivo = isAtivo;
        AuditInfo = auditInfo;
        UsuarioAdministradorId = usuarioAdministradorId;
    }
}
