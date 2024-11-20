using DDD.Contracts;
using Pulsar.Services.Catalog.Contracts.DTOs;
using Pulsar.Services.Catalog.Domain.Aggregates.Ineps;

namespace Pulsar.Services.Catalog.Domain.Aggregates.Regioes;

[BsonDiscriminator("Regiao", RootClass = true)]
[BsonKnownTypes(typeof(MunicipioRegiao), typeof(PaisRegiao), typeof(EstadoRegiao))]
public partial class Regiao : AggregateRoot
{
    public TipoLocal Tipo { get; set; }
    public long Codigo { get; set; }
    public string Nome { get; set; }
    public string TermosPesquisa { get; set; }
    public bool Ativo { get; set; }

    [BsonConstructor]
    public Regiao(ObjectId id, TipoLocal tipo, long codigo, string nome, string termosPesquisa, bool ativo) : base(id)
    {
        Id = id;
        Tipo = tipo;
        Codigo = codigo;
        Nome = nome;
        TermosPesquisa = termosPesquisa;
        Ativo = ativo;
    }

    public virtual RegiaoDTO ToDTO()
    {
        throw new NotImplementedException();
    }
}
