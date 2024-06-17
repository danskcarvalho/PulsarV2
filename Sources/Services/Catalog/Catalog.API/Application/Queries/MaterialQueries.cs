using MongoDB.Driver;
using Pulsar.BuildingBlocks.Caching;
using Pulsar.BuildingBlocks.DDD;
using Pulsar.Services.Catalog.Contracts.DTOs;
using Pulsar.Services.Catalog.Contracts.Enumerations;
using Pulsar.Services.Catalog.Contracts.Queries;
using Pulsar.Services.Catalog.Domain.Aggregates.Materiais;

namespace Pulsar.Services.Catalog.API.Application.Queries;

public class MaterialQueries : CatalogQueries, IMaterialQueries
{
    public MaterialQueries(CatalogQueriesContext ctx) : base(ctx)
    {
    }

    public async Task<List<MaterialDTO>> Find(string? filtro, TipoMaterial? tipo, string? principioAtivoId)
    {
        var key = new { Filtro = filtro, Tipo = tipo, PrincipioAtivoId = principioAtivoId };
        return await CacheServer.Category(CacheCategories.Find).Get(key.ToCacheKey(), async () =>
        {
            if (filtro.IsEmpty() || filtro?.Length <= 2)
            {
                return new List<MaterialDTO>();
            }

            var predicate = filtro.ToTextSearch<Material>();

            if (tipo != null)
            {
                predicate = Builders<Material>.Filter.Create(f => f.And(predicate, f.Eq(d => d.Tipo, tipo)));
            }
            if (principioAtivoId != null)
            {
                predicate = Builders<Material>.Filter.Create(f => f.And(predicate, f.Eq("PrincipioAtivo.PrincipioAtivoId", principioAtivoId.ToObjectId())));
            }
            var result = await MateriaisCollection.FindAsync(predicate, new FindOptions<Material, Material>()
            {
                Limit = 100
            }).ToListAsync();

            return result.Select(r => r.ToDTO()).ToList();
        });
    }

    public static class CacheCategories
    {
        public const string Find = "MaterialQueries.Find";
    }
}
