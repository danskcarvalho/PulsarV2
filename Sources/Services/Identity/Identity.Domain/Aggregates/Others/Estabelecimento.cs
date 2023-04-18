using Pulsar.BuildingBlocks.DDD;

namespace Pulsar.Services.Identity.Domain.Aggregates.Others;

public class Estabelecimento : AggregateRoot
{
    [BsonConstructor(nameof(Id), nameof(DominioId), nameof(Nome), nameof(Cnes), nameof(Redes), nameof(IsAtivo), nameof(AuditInfo), nameof(TimeStamp))]
    public Estabelecimento(ObjectId id, ObjectId dominioId, string nome, string cnes, IEnumerable<ObjectId> redes, bool isAtivo, AuditInfo auditInfo, DateTime timeStamp) : base(id)
    {
        DominioId = dominioId;
        Nome = nome;
        Cnes = cnes;
        Redes = new List<ObjectId>(redes);
        IsAtivo = isAtivo;
        TimeStamp = timeStamp;
        AuditInfo = auditInfo;
    }

    public ObjectId DominioId { get; private set; }
    public string Nome { get; set; }
    public string Cnes { get; set; }
    public List<ObjectId> Redes { get; private set; }
    public bool IsAtivo { get; set; }
    public AuditInfo AuditInfo { get; set; }
    public DateTime TimeStamp { get; set; }

    public bool Editar(string nome, string cnes, IEnumerable<ObjectId> redes, bool isAtivo, AuditInfo auditInfo, DateTime timeStamp, out long previousVersion)
    {
        previousVersion = Version;
        if (timeStamp <= this.TimeStamp)
            return false;

        this.Nome = nome;
        this.TimeStamp = timeStamp;
        this.AuditInfo = auditInfo;
        this.Redes = redes.ToList();
        this.IsAtivo = isAtivo;
        this.Cnes = cnes;
        this.Version++;
        return true;
    }
}
