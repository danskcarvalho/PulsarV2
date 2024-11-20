using DDD.Contracts;

namespace Pulsar.Services.Catalog.Domain.Aggregates.Dentes;

public partial class Dente : AggregateRoot
{
    public int Codigo { get; set; }
    public string Nome { get; set; }
    public string TermosPesquisa { get; set; }
    public bool Ativo { get; set; }

    [BsonConstructor]
    public Dente(ObjectId id, int codigo, string nome, bool ativo) : base(id)
    {
        Id = id;
        Codigo = codigo;
        Nome = nome;
        Ativo = ativo;
        TermosPesquisa = nome.Tokenize()!;
    }
}
