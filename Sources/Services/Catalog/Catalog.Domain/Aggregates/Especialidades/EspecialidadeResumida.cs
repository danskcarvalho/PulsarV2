namespace Pulsar.Services.Catalog.Domain.Aggregates.Especialidades;

public class EspecialidadeResumida
{
    public ObjectId EspecialidadeId { get; set; }
    public string Codigo { get; set; }
    public string Nome { get; set; }

    [BsonConstructor]
    public EspecialidadeResumida(ObjectId especialidadeId, string codigo, string nome)
    {
        EspecialidadeId = especialidadeId;
        Codigo = codigo;
        Nome = nome;
    }
}
