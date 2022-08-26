namespace Pulsar.Services.Identity.Domain.Aggregates.Grupos;

public class Grupo : AggregateRoot
{
    private string _termosBusca;
    private string _nome;

    public ObjectId DominioId { get; private set; }
    public string Nome
    {
        get => _nome; private set
        {
            _nome = value;
            if(!IsInitializing)
                _termosBusca = GetTermosBusca();
        }
    }
    public List<SubGrupo> SubGrupos { get; private set; }
    public AuditInfo AuditInfo { get; set; }
    public string TermosBusca
    {
        get => _termosBusca;
        private set => _termosBusca = value;
    }

    public int NumSubGrupos { get; set; }
    public int NumUsuarios { get; set; }

    [BsonConstructor(nameof(Id), nameof(DominioId), nameof(Nome), nameof(AuditInfo), nameof(SubGrupos))]
    public Grupo(ObjectId id, ObjectId dominioId, string nome, AuditInfo auditInfo, IEnumerable<SubGrupo>? subGrupos = null) : base(id)
    {
        _nome = nome;
        SubGrupos = subGrupos != null ? new List<SubGrupo>(subGrupos) : new List<SubGrupo>();
        AuditInfo = auditInfo;
        DominioId = dominioId;
        _termosBusca = GetTermosBusca();
    }

    private string GetTermosBusca()
    {
        return Nome.Tokenize()!;
    }
}
