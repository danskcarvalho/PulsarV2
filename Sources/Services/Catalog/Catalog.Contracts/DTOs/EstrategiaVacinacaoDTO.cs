namespace Pulsar.Services.Catalog.Contracts.DTOs;

public class EstrategiaVacinacaoDTO
{
    public int Codigo { get; set; }
    public string Nome { get; set; }
    public List<int> Doses { get; set; }

    public EstrategiaVacinacaoDTO(int codigo, string nome, List<int> doses)
    {
        Codigo = codigo;
        Nome = nome;
        Doses = doses;
    }
}
