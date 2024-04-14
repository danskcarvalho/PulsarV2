namespace Pulsar.Services.Catalog.Domain.Aggregates.Dentes;

public class Dente
{
    public ObjectId Id { get; set; }
    public int Codigo { get; set; }
    public string Nome { get; set; }
    public bool Ativo { get; set; }

    [BsonConstructor]
    public Dente(ObjectId id, int codigo, string nome, bool ativo)
    {
        Id = id;
        Codigo = codigo;
        Nome = nome;
        Ativo = ativo;
    }
}
