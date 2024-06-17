using Pulsar.BuildingBlocks.Caching;
using Pulsar.Services.Catalog.Contracts.DTOs;
using Pulsar.Services.Catalog.Contracts.Queries;
using Pulsar.Services.Catalog.Domain.Aggregates.Dentes;

namespace Pulsar.Services.Catalog.API.Application.Queries;

public class DenteQueries : CatalogQueries, IDenteQueries
{
    public DenteQueries(CatalogQueriesContext ctx) : base(ctx)
    {
    }

    public async Task<List<DenteDTO>> Find(string? filtro)
    {
        var key = new { Filtro = filtro };
        return await CacheServer.Category(CacheCategories.Find).Get(key.ToCacheKey(), async () =>
        {
            if (filtro.IsEmpty() || filtro?.Length <= 2)
            {
                return new List<DenteDTO>();
            }

            int codigo;
            if (!int.TryParse(filtro, out codigo))
            {
                codigo = -1;
            }

            var predicate = Builders<Dente>.Filter.Create(f =>
                    f.Or(filtro.ToTextSearch<Dente>(), f.Eq(d => d.Codigo, codigo)));
            var dentes = await DentesCollection.FindAsync(
                predicate,
                new MongoDB.Driver.FindOptions<Dente, Dente>
                {
                    Sort = Builders<Dente>.Sort.Ascending(d => d.Nome)
                }
            ).ToListAsync();

            return dentes.Select(d => new DenteDTO(d.Id.ToString(), d.Codigo, d.Nome)).ToList();
        });
        
    }

    public async Task<List<DenteDTO>> FindAll()
    {
        var key = (object?)null;
        return await CacheServer.Category(CacheCategories.FindAll).Get(key.ToCacheKey(), async () =>
        {
            var dentes = await DentesCollection.FindAsync<Dente>(
                Builders<Dente>.Filter.Empty,
                new MongoDB.Driver.FindOptions<Dente, Dente>
                {
                    Sort = Builders<Dente>.Sort.Ascending(d => d.Nome)
                }).ToListAsync();
            return dentes.Select(d => new DenteDTO(d.Id.ToString(), d.Codigo, d.Nome)).ToList();
        });
    }

    public static class CacheCategories
    {
        public const string Find = "DenteQueries.Find";
        public const string FindAll = "DenteQueries.FindAll";
    }
}
