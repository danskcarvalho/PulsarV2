namespace Pulsar.Services.Identity.Domain.Aggregates.Others;

public class Estabelecimento : AggregateRoot
{
    [BsonConstructor(nameof(Id), nameof(DominioId), nameof(Nome), nameof(Cnes))]
    public Estabelecimento(ObjectId id, ObjectId dominioId, string nome, string cnes) : base(id)
    {
        DominioId = dominioId;
        Nome = nome;
        Cnes = cnes;
        Redes = new List<ObjectId>();
    }

    public ObjectId DominioId { get; private set; }
    public string Nome { get; set; }
    public string Cnes { get; private set; }
    public List<ObjectId> Redes { get; private set; }
    public bool Ativo { get; set; }
    public DateTime TimeStamp { get; set; }
}
