using Pulsar.Services.Catalog.Contracts.DTOs;

namespace Pulsar.Services.Catalog.Domain.Aggregates.Regioes;

[BsonDiscriminator("EstadoRegiao")]
public class EstadoRegiao : Regiao
{
    public string Sigla { get; set; }
    public PaisResumido Pais { get; set; }

    [BsonConstructor]
    public EstadoRegiao(ObjectId id, TipoLocal tipo, long codigo, string nome, string termosPesquisa, bool ativo, string sigla, PaisResumido pais)
        : base(id, tipo, codigo, nome, termosPesquisa, ativo)
    {
        Sigla = sigla;
        Pais = pais;
    }

    public override RegiaoDTO ToDTO()
    {
        return new RegiaoDTO(Id.ToString(), TipoLocal.Estado, Codigo, Nome, Sigla);
    }
}
