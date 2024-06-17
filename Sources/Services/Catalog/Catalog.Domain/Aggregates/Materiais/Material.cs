using Pulsar.Services.Catalog.Contracts.DTOs;
using Pulsar.Services.Catalog.Domain.Aggregates.Especialidades;
using Pulsar.Services.Catalog.Domain.Aggregates.Regioes;

namespace Pulsar.Services.Catalog.Domain.Aggregates.Materiais;

[BsonDiscriminator("Material", RootClass = true)]
[BsonKnownTypes(typeof(Vacina), typeof(Medicamento))]
public partial class Material : AggregateRoot
{
    public string Nome { get; set; }
    public TipoMaterial Tipo { get; set; }
    public string TermosPesquisa { get; set; }
    public bool Ativo { get; set; }

    [BsonConstructor]
    public Material(ObjectId id, string nome, TipoMaterial tipo, string termosPesquisa, bool ativo) : base(id)
    {
        Id = id;
        Nome = nome;
        Tipo = tipo;
        TermosPesquisa = termosPesquisa;
        Ativo = ativo;
    }

    public virtual MaterialDTO ToDTO()
    {
        throw new NotImplementedException();
    }
}
