namespace Pulsar.Services.Catalog.Contracts.Queries;

public interface IMaterialQueries
{
    public Task<List<MaterialDTO>> Find(string? filtro, TipoMaterial? tipo);
}
