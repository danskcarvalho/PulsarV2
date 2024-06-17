namespace Pulsar.Services.Catalog.Contracts.Queries;

public interface IInepQueries
{
    public Task<List<InepDTO>> Find(string? filtro);
}
