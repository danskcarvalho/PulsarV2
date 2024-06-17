namespace Pulsar.Services.Catalog.Contracts.DTOs;

public class EtniaDTO
{
    public string Id { get; set; }
    public int Codigo { get; set; }
    public string Nome { get; set; }

    [JsonConstructor]
    public EtniaDTO(string id, int codigo, string nome)
    {
        Id = id;
        Codigo = codigo;
        Nome = nome;
    }
}
