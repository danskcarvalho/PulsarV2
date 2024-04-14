namespace Pulsar.Services.Catalog.Domain.Aggregates.Regioes;

public class PaisResumido
{
    public ObjectId PaisId { get; set; }
    public long Codigo { get; set; }
    public string Nome { get; set; }

    [BsonConstructor]
    public PaisResumido(ObjectId paisId, long codigo, string nome)
    {
        PaisId = paisId;
        Codigo = codigo;
        Nome = nome;
    }
}
