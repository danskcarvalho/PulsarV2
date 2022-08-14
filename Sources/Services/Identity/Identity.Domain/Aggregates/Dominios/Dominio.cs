namespace Pulsar.Services.Identity.Domain.Aggregates.Dominios;

public class Dominio : AggregateRoot
{
    public string Nome { get; set; }
    public bool IsAtivo { get; set; }
    public AuditInfo AuditInfo { get; set; }
    public ObjectId? UsuarioAdministradorId { get; set; }

    [BsonConstructor]
    public Dominio(ObjectId id, string nome, ObjectId? usuarioAdministradorId, AuditInfo auditInfo) : base(id)
    {
        Nome = nome;
        AuditInfo = auditInfo;
        UsuarioAdministradorId = usuarioAdministradorId;
    }
}
