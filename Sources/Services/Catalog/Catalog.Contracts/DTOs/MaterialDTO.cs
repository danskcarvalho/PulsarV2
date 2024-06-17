namespace Pulsar.Services.Catalog.Contracts.DTOs;

public class MaterialDTO
{
    public string Id { get; set; }
    public string Nome { get; set; }
    public TipoMaterial Tipo { get; set; }

    public MaterialDTO(string id, string nome, TipoMaterial tipo)
    {
        Nome = nome;
        Tipo = tipo;
    }
}
