namespace Pulsar.Services.Catalog.Domain.Aggregates.Regioes;

[BsonDiscriminator("MunicipioRegiao")]
public class MunicipioRegiao : Regiao
{
    public EstadoResumido Estado { get; set; }

    [BsonConstructor]
    public MunicipioRegiao(ObjectId id, TipoLocal tipo, long codigo, string nome, string termosPesquisa, bool ativo, EstadoResumido estado)
        : base(id, tipo, codigo, nome, termosPesquisa, ativo)
    {
        Estado = estado;
    }
}
