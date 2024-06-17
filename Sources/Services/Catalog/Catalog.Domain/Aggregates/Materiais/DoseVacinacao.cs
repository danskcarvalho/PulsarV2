
using Pulsar.Services.Catalog.Contracts.DTOs;

namespace Pulsar.Services.Catalog.Domain.Aggregates.Materiais;

public class DoseVacinacao
{
    public int Codigo { get; set; }
    public string Nome { get; set; }
    public HashSet<int> Estrategias { get; set; }

    [BsonConstructor]
    public DoseVacinacao(int codigo, string nome, HashSet<int> estrategias)
    {
        Codigo = codigo;
        Nome = nome;
        Estrategias = estrategias;
    }

    public DoseVacinacaoDTO ToDTO()
    {
        return new DoseVacinacaoDTO(Codigo, Nome, Estrategias.ToList());
    }
}
