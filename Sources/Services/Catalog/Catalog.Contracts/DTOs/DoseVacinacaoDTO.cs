namespace Pulsar.Services.Catalog.Contracts.DTOs;

public class DoseVacinacaoDTO
{
    public int Codigo { get; set; }
    public string Nome { get; set; }
    public List<int> Estrategias { get; set; }

    [JsonConstructor]
    public DoseVacinacaoDTO(int codigo, string nome, List<int> estrategias)
    {
        Codigo = codigo;
        Nome = nome;
        Estrategias = estrategias;
    }
}
