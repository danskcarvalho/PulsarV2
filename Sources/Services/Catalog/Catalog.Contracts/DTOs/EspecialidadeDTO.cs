namespace Pulsar.Services.Catalog.Contracts.DTOs;

public class EspecialidadeDTO
{
    public string Id { get; set; }
    public string Codigo { get; set; }
    public string Nome { get; set; }

    [JsonConstructor]
    public EspecialidadeDTO(string id, string codigo, string nome)
    {
        Codigo = codigo;
        Nome = nome;
        Id = id;
    }
}
