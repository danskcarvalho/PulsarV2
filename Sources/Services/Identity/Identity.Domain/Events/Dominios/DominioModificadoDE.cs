namespace Pulsar.Services.Identity.Domain.Events.Dominios;

public class DominioModificadoDE : INotification
{
    public ObjectId UsuarioLogadoId { get; set; }
    public ObjectId DominioId { get; set; }
    public string Nome { get; set; }
    public bool IsAtivo { get; set; }
    public AuditInfo AuditInfo { get; set; }
    public ObjectId? UsuarioAdministradorId { get; set; }
    public ObjectId? UsuarioAdministradorAnteriorId { get; set; }
    public ChangeEvent Modificacao { get; set; }

    public DominioModificadoDE(ObjectId usuarioLogadoId, ObjectId dominioId, string nome, bool isAtivo, AuditInfo auditInfo, ObjectId? usuarioAdministradorId, ObjectId? usuarioAdministradorAnteriorId, ChangeEvent modificacao)
    {
        UsuarioLogadoId = usuarioLogadoId;
        DominioId = dominioId;
        Nome = nome;
        IsAtivo = isAtivo;
        AuditInfo = auditInfo;
        UsuarioAdministradorId = usuarioAdministradorId;
        UsuarioAdministradorAnteriorId = usuarioAdministradorAnteriorId;
        Modificacao = modificacao;
    }
}
