using Pulsar.Services.Identity.Domain.Events.Others;

namespace Pulsar.Services.Identity.Domain.Aggregates.Others;

public class RedeEstabelecimentos : AggregateRoot
{
    [BsonConstructor]
    public RedeEstabelecimentos(ObjectId id, ObjectId dominioId, string nome, AuditInfo auditInfo, DateTime timeStamp) : base(id)
    {
        Nome = nome ?? throw new ArgumentNullException(nameof(nome));
        TimeStamp = timeStamp;
        DominioId = dominioId;
        AuditInfo = auditInfo;
    }


    public ObjectId DominioId { get; private set; }
    public string Nome { get; set; }
    public AuditInfo AuditInfo { get; set; }
    public DateTime TimeStamp { get; set; }

    public bool Editar(string nome, AuditInfo auditInfo, DateTime timeStamp, out long previousVersion)
    {
        previousVersion = Version;
        if (timeStamp <= this.TimeStamp)
            return false;

        this.Nome = nome;
        this.TimeStamp = timeStamp;
        this.AuditInfo = auditInfo;
        this.Version++;
        if (this.AuditInfo.RemovidoEm != null)
            this.AddDomainEvent(new RedeEstabelecimentosRemovidaDomainEvent(this.Id));
        return true;
    }
}
