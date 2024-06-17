namespace Pulsar.Services.Catalog.Contracts.DTOs;

public class PrincipioAtivoDTO
{
    public string Id { get; set; }
    public string Nome { get; set; }
    public string CodigoEsus { get; set; }
    public CategoriaMedicamento Categoria { get; set; }
    public TipoMedicamento Tipo { get; set; }

    public PrincipioAtivoDTO(string id, string nome, string codigoEsus, CategoriaMedicamento categoria, TipoMedicamento tipo)
    {
        Id = id;
        Nome = nome;
        CodigoEsus = codigoEsus;
        Categoria = categoria;
        Tipo = tipo;
    }
}
