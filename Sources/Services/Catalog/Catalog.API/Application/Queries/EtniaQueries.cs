using MongoDB.Driver;
using Pulsar.BuildingBlocks.Caching;
using Pulsar.Services.Catalog.Contracts.DTOs;
using Pulsar.Services.Catalog.Contracts.Queries;
using Pulsar.Services.Catalog.Domain.Aggregates.Etnias;

namespace Pulsar.Services.Catalog.API.Application.Queries;

public class EtniaQueries : CatalogQueries, IEtniaQueries
{
    public EtniaQueries(CatalogQueriesContext ctx) : base(ctx)
    {
    }

    public async Task<List<EtniaDTO>> Find(string? filtro)
    {
        var key = new { Filtro = filtro };
        return await CacheServer.Category(CacheCategories.Find).Get(key.ToCacheKey(), async () =>
        {
            if (filtro.IsEmpty() || filtro?.Length <= 2)
            {
                return new List<EtniaDTO>();
            }

            var codigo = -1;
            if (!int.TryParse(filtro, out codigo))
            {
                codigo = -1;
            }

            var predicate = Builders<Etnia>.Filter.Create(f =>
                f.Or(filtro.ToTextSearch<Etnia>(), f.Eq(d => d.Codigo, codigo))
            );

            var result = await EtniasCollection.FindAsync(predicate, new FindOptions<Etnia, Etnia>()
            {
                Limit = 100
            }).ToListAsync();

            return result.Select(r => r.ToDTO()).ToList();
        });
    }

    public static class CacheCategories
    {
        public const string Find = "EtniaQueries.Find";
    }
}
