namespace Pulsar.Services.Catalog.Contracts.DTOs;

public class MedicamentoUnidadeFornecimentoDTO
{
    public string Id { get; set; }
    public string Nome { get; set; }
    public string CodigoEsus { get; set; }

    public MedicamentoUnidadeFornecimentoDTO(string id, string nome, string codigoEsus)
    {
        Id = id;
        Nome = nome;
        CodigoEsus = codigoEsus;
    }
}
