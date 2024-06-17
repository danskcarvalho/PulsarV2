namespace Pulsar.Services.Catalog.Contracts.Queries;

public interface IEtniaQueries
{
    public Task<List<EtniaDTO>> Find(string? filtro);
}
