using MongoDB.Driver;
using Pulsar.BuildingBlocks.Caching;
using Pulsar.Services.Catalog.Contracts.DTOs;
using Pulsar.Services.Catalog.Contracts.Queries;
using Pulsar.Services.Catalog.Domain.Aggregates.PrincipiosAtivos;

namespace Pulsar.Services.Catalog.API.Application.Queries;

public class PrincipioAtivoQueries : CatalogQueries, IPrincipioAtivoQueries
{
    public PrincipioAtivoQueries(CatalogQueriesContext ctx) : base(ctx)
    {
    }

    public async Task<List<PrincipioAtivoDTO>> Find(string? filtro)
    {
        var key = new { Filtro = filtro };
        return await CacheServer.Category(CacheCategories.Find).Get(key.ToCacheKey(), async () =>
        {
            if (filtro.IsEmpty() || filtro?.Length <= 2)
            {
                return new List<PrincipioAtivoDTO>();
            }

            var codigo = filtro?.Trim();

            var predicate = Builders<PrincipioAtivo>.Filter.Create(f =>
                f.Or(filtro.ToTextSearch<PrincipioAtivo>(), f.Eq(d => d.CodigoEsus, codigo))
            );

            var result = await PrincipiosAtivosCollection.FindAsync(predicate, new FindOptions<PrincipioAtivo, PrincipioAtivo>()
            {
                Limit = 100
            }).ToListAsync();

            return result.Select(r => r.ToDTO()).ToList();
        });
    }

    public static class CacheCategories
    {
        public const string Find = "PrincipioAtivoQueries.Find";
    }
}
