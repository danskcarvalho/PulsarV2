using MongoDB.Driver;
using Pulsar.BuildingBlocks.Caching;
using Pulsar.Services.Catalog.Contracts.DTOs;
using Pulsar.Services.Catalog.Contracts.Enumerations;
using Pulsar.Services.Catalog.Contracts.Queries;
using Pulsar.Services.Catalog.Domain.Aggregates.Diagnosticos;
using Pulsar.Services.Shared.Enumerations;

namespace Pulsar.Services.Catalog.API.Application.Queries;

public class DiagnosticoQueries : CatalogQueries, IDiagnosticoQueries
{
    public DiagnosticoQueries(CatalogQueriesContext ctx) : base(ctx)
    {
    }

    public async Task<List<DiagnosticoDTO>> Find(string? filtro, Sexo? sexo, TipoDiagnostico? tipo)
    {
        var key = new { Filtro = filtro, Sexo = sexo, Tipo = tipo };
        return await CacheServer.Category(CacheCategories.Find).Get(key.ToCacheKey(), async () =>
        {
            if (filtro.IsEmpty() || filtro?.Length <= 2)
            {
                return new List<DiagnosticoDTO>();
            }

            var codigo = filtro?.Trim();

            var predicate = Filters.Diagnosticos.Create(f => 
                f.Or(filtro.ToTextSearch<Diagnostico>(), f.Eq(d => d.Codigo, codigo))
            );

            if (sexo != null)
            {
                predicate = Filters.Diagnosticos.Create(f => f.And(predicate, f.Or(f.Eq(d => d.Sexo, sexo), f.Eq(d => d.Sexo, null))));
            }
            if (tipo != null)
            {
                predicate = Filters.Diagnosticos.Create(f => f.And(predicate, f.Eq(d => d.Tipo, tipo)));
            }
            return await DiagnosticosCollection.FindAsync(predicate, new FindOptions<Diagnostico, DiagnosticoDTO>()
            {
                Projection = Builders<Diagnostico>.Projection.Expression(d => new DiagnosticoDTO(d.Id.ToString(), d.Tipo, d.Codigo, d.Nome, d.Sexo))
            }).ToListAsync();
        });
    }

    public static class CacheCategories
    {
        public const string Find = "DiagnosticoQueries.Find";
    }
}
