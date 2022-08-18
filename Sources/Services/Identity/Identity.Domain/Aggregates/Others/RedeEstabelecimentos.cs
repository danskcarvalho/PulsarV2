namespace Pulsar.Services.Identity.Domain.Aggregates.Others;

public class RedeEstabelecimentos : AggregateRoot
{
    [BsonConstructor]
    public RedeEstabelecimentos(ObjectId id, ObjectId dominioId, string nome, DateTime timeStamp) : base(id)
    {
        Nome = nome ?? throw new ArgumentNullException(nameof(nome));
        TimeStamp = timeStamp;
        DominioId = dominioId;
    }


    public ObjectId DominioId { get; private set; }
    public string Nome { get; set; }
    public DateTime TimeStamp { get; set; }
}
