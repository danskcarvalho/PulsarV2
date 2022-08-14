namespace Pulsar.Services.Identity.Domain.Aggregates.Others;

public class Estabelecimento : AggregateRoot
{
    [BsonConstructor(nameof(Id), nameof(DominioId), nameof(Nome), nameof(Cnes), nameof(Redes), nameof(TimeStamp))]
    public Estabelecimento(ObjectId id, ObjectId dominioId, string nome, string cnes, IEnumerable<ObjectId> redes, DateTime timeStamp) : base(id)
    {
        DominioId = dominioId;
        Nome = nome;
        Cnes = cnes;
        TimeStamp = timeStamp;
        Redes = redes.ToList().AsReadOnly();
    }

    public ObjectId DominioId { get; private set; }
    public string Nome { get; private set; }
    public string Cnes { get; private set; }
    public IReadOnlyList<ObjectId> Redes { get; private set; }
    public DateTime TimeStamp { get; private set; }
}
