using MongoDB.Driver;
using Pulsar.BuildingBlocks.Caching;
using Pulsar.Services.Catalog.Contracts.DTOs;
using Pulsar.Services.Catalog.Contracts.Queries;
using Pulsar.Services.Catalog.Domain.Aggregates.Especialidades;

namespace Pulsar.Services.Catalog.API.Application.Queries;

public class EspecialidadeQueries : CatalogQueries, IEspecialidadeQueries
{
    public EspecialidadeQueries(CatalogQueriesContext ctx) : base(ctx)
    {
    }

    public async Task<List<EspecialidadeDTO>> Find(string? filtro)
    {
        var key = new { Filtro = filtro };
        return await CacheServer.Category(CacheCategories.Find).Get(key.ToCacheKey(), async () =>
        {
            if (filtro.IsEmpty() || filtro?.Length <= 2)
            {
                return new List<EspecialidadeDTO>();
            }

            var codigo = filtro?.Trim();

            var predicate = Builders<Especialidade>.Filter.Create(f =>
                f.Or(filtro.ToTextSearch<Especialidade>(), f.Eq(d => d.Codigo, codigo))
            );

            var result = await EspecialidadesCollection.FindAsync(predicate, new FindOptions<Especialidade, Especialidade>()
            {
                Limit = 100
            }).ToListAsync();

            return result.Select(r => r.ToDTO()).ToList();
        });
    }

    public static class CacheCategories
    {
        public const string Find = "EspecialidadeQueries.Find";
    }
}
