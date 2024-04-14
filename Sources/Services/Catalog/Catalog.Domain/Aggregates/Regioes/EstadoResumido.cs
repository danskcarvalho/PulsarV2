namespace Pulsar.Services.Catalog.Domain.Aggregates.Regioes;

public class EstadoResumido
{
    public ObjectId EstadoId { get; set; }
    public long Codigo { get; set; }
    public string Nome { get; set; }
    public string Sigla { get; set; }
    public PaisResumido Pais { get; set; }

    [BsonConstructor]
    public EstadoResumido(ObjectId estadoId, long codigo, string nome, string sigla, PaisResumido pais)
    {
        EstadoId = estadoId;
        Codigo = codigo;
        Nome = nome;
        Sigla = sigla;
        Pais = pais;
    }
}
