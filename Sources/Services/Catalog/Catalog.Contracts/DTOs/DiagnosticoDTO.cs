using Pulsar.Services.Shared.Enumerations;

namespace Pulsar.Services.Catalog.Contracts.DTOs;

public class DiagnosticoDTO
{
    [JsonConstructor]
    public DiagnosticoDTO(string id, TipoDiagnostico tipo, string codigo, string nome, Sexo? sexo)
    {
        Id = id;
        Tipo = tipo;
        Codigo = codigo;
        Nome = nome;
        Sexo = sexo;
    }

    public string Id { get; set; }
    public TipoDiagnostico Tipo { get; set; }
    public string Codigo { get; set; }
    public string Nome { get; set; }
    public Sexo? Sexo { get; set; }
}
