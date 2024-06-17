using MongoDB.Driver;
using Pulsar.BuildingBlocks.Caching;
using Pulsar.BuildingBlocks.DDD;
using Pulsar.Services.Catalog.Contracts.DTOs;
using Pulsar.Services.Catalog.Contracts.Enumerations;
using Pulsar.Services.Catalog.Contracts.Queries;
using Pulsar.Services.Catalog.Domain.Aggregates.Regioes;

namespace Pulsar.Services.Catalog.API.Application.Queries;

public class RegiaoQueries : CatalogQueries, IRegiaoQueries
{
    public RegiaoQueries(CatalogQueriesContext ctx) : base(ctx)
    {
    }

    public async Task<List<RegiaoDTO>> Find(string? filtro, TipoLocal? tipo, string? regiaoPaiId)
    {
        var key = new { Filtro = filtro, Tipo = tipo, RegiaoPaiId = regiaoPaiId };
        return await CacheServer.Category(CacheCategories.Find).Get(key.ToCacheKey(), async () =>
        {
            if (filtro.IsEmpty() || filtro?.Length <= 2)
            {
                return new List<RegiaoDTO>();
            }

            var codigo = filtro?.Trim();

            var predicate = filtro.ToTextSearch<Regiao>();

            if (tipo != null)
            {
                predicate = Builders<Regiao>.Filter.Create(f => f.And(predicate, f.Eq(d => d.Tipo, tipo)));
            }
            if (regiaoPaiId != null)
            {
                predicate = Builders<Regiao>.Filter.Create(f => f.And(predicate, f.Or(f.Eq("Estado.EstadoId", regiaoPaiId.ToObjectId()), f.Eq("Pais.PaisId", regiaoPaiId.ToObjectId()))));
            }
            var result = await RegioesCollection.FindAsync(predicate, new FindOptions<Regiao, Regiao>()
            {
                Limit = 100
            }).ToListAsync();

            return result.Select(r => r.ToDTO()).ToList();
        });
    }

    public static class CacheCategories
    {
        public const string Find = "RegiaoQueries.Find";
    }
}
