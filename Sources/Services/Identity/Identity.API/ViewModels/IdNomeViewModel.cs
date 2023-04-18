using System.Diagnostics.CodeAnalysis;

namespace Pulsar.Services.Identity.API.ViewModels;

public class IdNomeViewModel
{
    /// <summary>
    /// Id da entidade.
    /// </summary>
    public required string Id { get; set; }
    /// <summary>
    /// Nome da entidade.
    /// </summary>
    public required string Nome { get; set; }

    [SetsRequiredMembers]
    public IdNomeViewModel(string id, string nome)
    {
        Id = id;
        Nome = nome;
    }

    private IdNomeViewModel() { }
}
