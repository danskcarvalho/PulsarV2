namespace Pulsar.Services.Catalog.Domain.Aggregates.Regioes;

public class MunicipioResumido
{
    public ObjectId MunicipioId { get; set; }
    public long Codigo { get; set; }
    public string Nome { get; set; }
    public EstadoResumido Estado { get; set; }

    [BsonConstructor]
    public MunicipioResumido(ObjectId municipioId, long codigo, string nome, EstadoResumido estado)
    {
        MunicipioId = municipioId;
        Codigo = codigo;
        Nome = nome;
        Estado = estado;
    }
}
