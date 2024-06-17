namespace Pulsar.Services.Catalog.Contracts.Queries;

public interface IRegiaoQueries
{
    public Task<List<RegiaoDTO>> Find(string? filtro, TipoLocal? tipo, string? regiaoPaiId);
}
