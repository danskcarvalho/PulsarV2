using Pulsar.Services.Shared.Enumerations;

namespace Pulsar.Services.Catalog.Contracts.Queries;

public interface IEspecialidadeQueries
{
    public Task<List<EspecialidadeDTO>> Find(string? filtro);
}
