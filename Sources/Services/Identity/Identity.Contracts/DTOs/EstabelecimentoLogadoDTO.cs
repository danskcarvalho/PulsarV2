using System.Diagnostics.CodeAnalysis;

namespace Pulsar.Services.Identity.Contracts.DTOs;

public class EstabelecimentoLogadoDTO
{
    [SetsRequiredMembers]
    public EstabelecimentoLogadoDTO(string id, string nome)
    {
        Id = id;
        Nome = nome;
    }

    private EstabelecimentoLogadoDTO() { }

    /// <summary>
    /// Id do estabelecimento.
    /// </summary>
    public required string Id { get; set; }
    /// <summary>
    /// Nome do estabelecimento.
    /// </summary>
    public required string Nome { get; set; }
}
