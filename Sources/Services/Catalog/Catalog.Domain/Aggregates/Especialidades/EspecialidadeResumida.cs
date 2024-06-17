using Pulsar.Services.Catalog.Contracts.DTOs;

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

    public EspecialidadeDTO ToDTO()
    {
        return new EspecialidadeDTO(EspecialidadeId.ToString(), Codigo, Nome);
    }
}
