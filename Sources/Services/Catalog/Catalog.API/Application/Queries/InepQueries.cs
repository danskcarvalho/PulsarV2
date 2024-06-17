using MongoDB.Driver;
using Pulsar.BuildingBlocks.Caching;
using Pulsar.Services.Catalog.Contracts.DTOs;
using Pulsar.Services.Catalog.Contracts.Queries;
using Pulsar.Services.Catalog.Domain.Aggregates.Ineps;

namespace Pulsar.Services.Catalog.API.Application.Queries;

public class InepQueries : CatalogQueries, IInepQueries
{
    public InepQueries(CatalogQueriesContext ctx) : base(ctx)
    {
    }

    public async Task<List<InepDTO>> Find(string? filtro)
    {
        var key = new { Filtro = filtro };
        return await CacheServer.Category(CacheCategories.Find).Get(key.ToCacheKey(), async () =>
        {
            if (filtro.IsEmpty() || filtro?.Length <= 2)
            {
                return new List<InepDTO>();
            }

            var codigo = filtro?.Trim();

            var predicate = Builders<Inep>.Filter.Create(f =>
                f.Or(filtro.ToTextSearch<Inep>(), f.Eq(d => d.Codigo, codigo))
            );

            var result = await InepsCollection.FindAsync(predicate, new FindOptions<Inep, Inep>()
            {
                Limit = 100
            }).ToListAsync();

            return result.Select(r => r.ToDTO()).ToList();
        });
    }

    public static class CacheCategories
    {
        public const string Find = "InepQueries.Find";
    }
}
