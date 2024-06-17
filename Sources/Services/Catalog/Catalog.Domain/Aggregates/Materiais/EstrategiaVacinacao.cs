
using Pulsar.Services.Catalog.Contracts.DTOs;

namespace Pulsar.Services.Catalog.Domain.Aggregates.Materiais;

public class EstrategiaVacinacao
{
    public int Codigo { get; set; }
    public string Nome { get; set; }
    public HashSet<int> Doses { get; set; }

    [BsonConstructor]
    public EstrategiaVacinacao(int codigo, string nome, HashSet<int> doses)
    {
        Codigo = codigo;
        Nome = nome;
        Doses = doses;
    }

    public EstrategiaVacinacaoDTO ToDTO()
    {
        return new EstrategiaVacinacaoDTO(Codigo, Nome, Doses.ToList());
    }
}
