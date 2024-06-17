using MongoDB.Driver;
using Pulsar.BuildingBlocks.Caching;
using Pulsar.Services.Catalog.Contracts.DTOs;
using Pulsar.Services.Catalog.Contracts.Queries;
using Pulsar.Services.Catalog.Domain.Aggregates.Procedimentos;
using Pulsar.Services.Shared.Enumerations;

namespace Pulsar.Services.Catalog.API.Application.Queries;

public class ProcedimentoQueries : CatalogQueries, IProcedimentoQueries
{
    public ProcedimentoQueries(CatalogQueriesContext ctx) : base(ctx)
    {
    }

    public async Task<List<ProcedimentoDTO>> Find(string? filtro, Sexo? sexo, Complexidade? complexidade, int? idadeEmDias, bool? procedimentoAb)
    {
        var key = new { Filtro = filtro, Sexo = sexo, Complexidade = complexidade, IdadeEmDias = idadeEmDias, ProcedimentoAb = procedimentoAb };
        return await CacheServer.Category(CacheCategories.Find).Get(key.ToCacheKey(), async () =>
        {
            if (filtro.IsEmpty() || filtro?.Length <= 2)
            {
                return new List<ProcedimentoDTO>();
            }

            var codigo = filtro?.Trim();

            var predicate = Builders<Procedimento>.Filter.Create(f => f.Or(filtro.ToTextSearch<Procedimento>(), f.Eq(p => p.Codigo, codigo)));

            if (sexo != null)
            {
                predicate = Builders<Procedimento>.Filter.Create(f => f.And(predicate, f.Eq(d => d.Sexo, sexo)));
            }
            if (complexidade != null)
            {
                predicate = Builders<Procedimento>.Filter.Create(f => f.And(predicate, f.Eq(d => d.Complexidade, complexidade)));
            }
            if (idadeEmDias != null)
            {
                predicate = Builders<Procedimento>.Filter.Create(f => f.And(predicate,
                    f.Lte(d => d.IdadeMinimaEmDias, idadeEmDias), 
                    f.Gte(d => d.IdadeMaximaEmDias, idadeEmDias)));
            }
            if (procedimentoAb != null)
            {
                predicate = Builders<Procedimento>.Filter.Create(f => f.And(predicate, f.Eq(d => d.ProcedimentoAb, procedimentoAb)));
            }
            var result = await ProcedimentosCollection.FindAsync(predicate, new FindOptions<Procedimento, Procedimento>()
            {
                Limit = 100
            }).ToListAsync();

            return result.Select(r => r.ToDTO()).ToList();
        });
    }

    public static class CacheCategories
    {
        public const string Find = "ProcedimentoQueries.Find";
    }
}
