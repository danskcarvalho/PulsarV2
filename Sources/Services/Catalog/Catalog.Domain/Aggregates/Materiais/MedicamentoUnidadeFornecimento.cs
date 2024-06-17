
using Pulsar.Services.Catalog.Contracts.DTOs;

namespace Pulsar.Services.Catalog.Domain.Aggregates.Materiais;
public class MedicamentoUnidadeFornecimento
{
    public ObjectId UnidadeFornecimentoId { get; set; }
    public string Nome { get; set; }
    public string CodigoEsus { get; set; }

    [BsonConstructor]
    public MedicamentoUnidadeFornecimento(ObjectId unidadeFornecimentoId, string nome, string codigoEsus)
    {
        UnidadeFornecimentoId = unidadeFornecimentoId;
        Nome = nome;
        CodigoEsus = codigoEsus;
    }

    public MedicamentoUnidadeFornecimentoDTO ToDTO()
    {
        return new MedicamentoUnidadeFornecimentoDTO(UnidadeFornecimentoId.ToString(), Nome, CodigoEsus);
    }
}
