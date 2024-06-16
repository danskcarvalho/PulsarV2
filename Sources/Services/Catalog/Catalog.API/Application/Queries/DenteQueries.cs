using Pulsar.Services.Catalog.Contracts.DTOs;
using Pulsar.Services.Catalog.Contracts.Queries;
using Pulsar.Services.Catalog.Domain.Aggregates.Dentes;
using System.Linq;

namespace Pulsar.Services.Catalog.API.Application.Queries;

public class DenteQueries : CatalogQueries, IDenteQueries
{
    public DenteQueries(CatalogQueriesContext ctx) : base(ctx)
    {
    }

    public async Task<List<DenteDTO>> Find(string? filtro)
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

        var dentes = await DentesCollection.FindAsync<Dente>(
            Filters.Dentes.Create(f => 
                f.Or(filtro.ToTextSearch<Dente>(), f.Eq(d => d.Codigo, codigo)))
        ).ToListAsync();

        return dentes.Select(d => new DenteDTO(d.Id.ToString(), d.Codigo, d.Nome)).ToList();
    }

    public async Task<List<DenteDTO>> FindAll()
    {
        var dentes = await DentesCollection.FindAsync<Dente>(Filters.Dentes.Empty).ToListAsync();
        return dentes.Select(d => new DenteDTO(d.Id.ToString(), d.Codigo, d.Nome)).ToList();
    }
}
