using DDD.Contracts;
using Pulsar.Services.Catalog.Contracts.DTOs;
using Pulsar.Services.Catalog.Domain.Aggregates.Ineps;

namespace Pulsar.Services.Catalog.Domain.Aggregates.PrincipiosAtivos;

public partial class PrincipioAtivo : AggregateRoot
{
    public string Nome { get; set; }
    public string CodigoEsus { get; set; }
    public CategoriaMedicamento Categoria { get; set; }
    public TipoMedicamento Tipo { get; set; }
    public string TermosPesquisa { get; set; }

    [BsonConstructor]
    public PrincipioAtivo(ObjectId id, string nome, string codigoEsus, CategoriaMedicamento categoria, TipoMedicamento tipo, string termosPesquisa) : base(id)
    {
        Id = id;
        Nome = nome;
        CodigoEsus = codigoEsus;
        Categoria = categoria;
        Tipo = tipo;
        TermosPesquisa = termosPesquisa;
    }

    public PrincipioAtivoDTO ToDTO()
    {
        return new PrincipioAtivoDTO(Id.ToString(), Nome, CodigoEsus, Categoria, Tipo);
    }
}
