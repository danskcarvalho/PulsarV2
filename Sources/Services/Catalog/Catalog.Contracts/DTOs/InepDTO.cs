namespace Pulsar.Services.Catalog.Contracts.DTOs;

public class InepDTO
{
    public string Id { get; set; }
    public string Codigo { get; set; }
    public string Nome { get; set; }

    public InepDTO(string id, string codigo, string nome)
    {
        Id = id;
        Codigo = codigo;
        Nome = nome;
    }
}
