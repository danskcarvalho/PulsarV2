namespace Pulsar.Services.Catalog.Contracts.Queries;

public interface IPrincipioAtivoQueries
{
    public Task<List<PrincipioAtivoDTO>> Find(string? filtro);
}
