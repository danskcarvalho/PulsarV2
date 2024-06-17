namespace Pulsar.Services.Catalog.Contracts.DTOs;

public class RegiaoDTO
{
    public string Id { get; set; }
    public TipoLocal Tipo { get; set; }
    public long Codigo { get; set; }
    public string Nome { get; set; }
    public string? Sigla { get; set; }

    public RegiaoDTO(string id, TipoLocal tipo, long codigo, string nome, string? sigla)
    {
        Id = id;
        Tipo = tipo;
        Codigo = codigo;
        Nome = nome;
        Sigla = sigla;
    }
}
