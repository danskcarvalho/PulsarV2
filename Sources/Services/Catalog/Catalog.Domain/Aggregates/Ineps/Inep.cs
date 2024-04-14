using Pulsar.Services.Catalog.Domain.Aggregates.Regioes;

namespace Pulsar.Services.Catalog.Domain.Aggregates.Ineps;

public class Inep
{
    public ObjectId Id { get; set; }
    public string Codigo { get; set; }
    public string Nome { get; set; }
    public string TermosPesquisa { get; set; }
    public MunicipioResumido Municipio { get; set; }
    public bool Ativo { get; set; }

    [BsonConstructor]
    public Inep(ObjectId id, string codigo, string nome, string termosPesquisa, MunicipioResumido municipio, bool ativo)
    {
        Id = id;
        Codigo = codigo;
        Nome = nome;
        TermosPesquisa = termosPesquisa;
        Municipio = municipio;
        Ativo = ativo;
    }
}
