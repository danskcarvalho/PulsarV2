using DDD.Contracts;
using Pulsar.Services.Catalog.Contracts.DTOs;
using Pulsar.Services.Catalog.Domain.Aggregates.Especialidades;

namespace Pulsar.Services.Catalog.Domain.Aggregates.Etnias;

public partial class Etnia : AggregateRoot
{
    public int Codigo { get; set; }
    public string Nome { get; set; }
    public string TermosPesquisa { get; set; }
    public bool Ativo { get; set; }

    [BsonConstructor]
    public Etnia(ObjectId id, int codigo, string nome, string termosPesquisa, bool ativo) : base(id)
    {
        Id = id;
        Codigo = codigo;
        Nome = nome;
        TermosPesquisa = termosPesquisa;
        Ativo = ativo;
    }

    public EtniaDTO ToDTO()
    {
        return new EtniaDTO(Id.ToString(), Codigo, Nome);
    }
}
