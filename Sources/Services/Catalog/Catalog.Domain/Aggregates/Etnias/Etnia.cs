namespace Pulsar.Services.Catalog.Domain.Aggregates.Etnias;

public class Etnia
{
    public ObjectId Id { get; set; }
    public int Codigo { get; set; }
    public string Nome { get; set; }
    public string TermosPesquisa { get; set; }
    public bool Ativo { get; set; }

    [BsonConstructor]
    public Etnia(ObjectId id, int codigo, string nome, string termosPesquisa, bool ativo)
    {
        Id = id;
        Codigo = codigo;
        Nome = nome;
        TermosPesquisa = termosPesquisa;
        Ativo = ativo;
    }
}
