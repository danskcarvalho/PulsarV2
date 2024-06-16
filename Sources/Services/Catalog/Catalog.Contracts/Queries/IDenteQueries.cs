namespace Pulsar.Services.Catalog.Contracts.Queries;

public interface IDenteQueries
{
    public Task<List<DenteDTO>> Find(string? filtro);
    public Task<List<DenteDTO>> FindAll();
}
