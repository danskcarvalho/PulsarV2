using Pulsar.Services.Identity.Domain.Aggregates.Grupos;

namespace Pulsar.Services.Identity.Domain.Events.Grupos;

public class GrupoModificadoDomainEvent : INotification
{
    public ObjectId UsuarioLogadoId { get; set; }
    public ObjectId DominioId { get; set; }
    public ObjectId GrupoId { get; set; }
    public string Nome { get; set; }
    public long Version { get; set; }
    public AuditInfo AuditInfo { get; set; }
    public List<SubGrupo> SubGruposModificados { get; set; }
    public List<SubGrupo> SubGruposAdicionados { get; set; }
    public List<SubGrupo> SubGruposRemovidos { get; set; }
    public ChangeEvent Modificacao { get; set; }

    public GrupoModificadoDomainEvent(ObjectId usuarioLogadoId, ObjectId dominioId, ObjectId grupoId, string nome, AuditInfo auditInfo, List<SubGrupo> subGruposModificados, List<SubGrupo> subGruposAdicionados, List<SubGrupo> subGruposRemovidos, ChangeEvent modificacao, long version)
    {
        UsuarioLogadoId = usuarioLogadoId;
        GrupoId = grupoId;
        Nome = nome;
        AuditInfo = auditInfo;
        SubGruposModificados = subGruposModificados;
        SubGruposAdicionados = subGruposAdicionados;
        SubGruposRemovidos = subGruposRemovidos;
        Modificacao = modificacao;
        Version = version;
        DominioId = dominioId;
    }
}
