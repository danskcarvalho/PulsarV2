using Pulsar.Services.Catalog.Contracts.DTOs;

namespace Pulsar.Services.Catalog.Domain.Aggregates.Regioes;

[BsonDiscriminator("PaisRegiao")]
public class PaisRegiao : Regiao
{
    [BsonConstructor]
    public PaisRegiao(ObjectId id, TipoLocal tipo, long codigo, string nome, string termosPesquisa, bool ativo)
        : base(id, tipo, codigo, nome, termosPesquisa, ativo)
    {

    }

    public override RegiaoDTO ToDTO()
    {
        return new RegiaoDTO(Id.ToString(), TipoLocal.Pais, Codigo, Nome, null);
    }
}
