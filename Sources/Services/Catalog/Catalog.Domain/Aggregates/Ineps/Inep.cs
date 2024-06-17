using Pulsar.Services.Catalog.Contracts.DTOs;
using Pulsar.Services.Catalog.Domain.Aggregates.Especialidades;
using Pulsar.Services.Catalog.Domain.Aggregates.Regioes;

namespace Pulsar.Services.Catalog.Domain.Aggregates.Ineps;

public partial class Inep : AggregateRoot
{
    public string Codigo { get; set; }
    public string Nome { get; set; }
    public string TermosPesquisa { get; set; }
    public MunicipioResumido Municipio { get; set; }
    public bool Ativo { get; set; }

    [BsonConstructor]
    public Inep(ObjectId id, string codigo, string nome, string termosPesquisa, MunicipioResumido municipio, bool ativo) : base(id)
    {
        Id = id;
        Codigo = codigo;
        Nome = nome;
        TermosPesquisa = termosPesquisa;
        Municipio = municipio;
        Ativo = ativo;
    }

    public InepDTO ToDTO()
    {
        return new InepDTO(Id.ToString(), Codigo, Nome);
    }
}
