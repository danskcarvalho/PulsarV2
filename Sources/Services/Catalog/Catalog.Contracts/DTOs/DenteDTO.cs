namespace Pulsar.Services.Catalog.Contracts.DTOs;

public class DenteDTO
{
    /// <summary>
    /// Id do dente.
    /// </summary>
    public string Id { get; set; }
    /// <summary>
    /// Código do dente.
    /// </summary>
    public int Codigo { get; set; }
    /// <summary>
    /// Nome do dente.
    /// </summary>
    public string Nome { get; set; }

    [JsonConstructor]
    public DenteDTO(string id, int codigo, string nome)
    {
        Id = id;
        Codigo = codigo;
        Nome = nome;
    }
}
