using Pulsar.Services.Identity.Contracts.DTOs;
using Pulsar.Services.Identity.Contracts.Queries;
using Pulsar.Services.Shared.DTOs;

namespace Identity.UnitTests.Queries;

public class MockedDominioQueries : IDominioQueries
{
    public Task<PaginatedListDTO<DominioListadoDTO>> FindDominios(string? filtro, bool? showHidden, string? cursor, int? limit, string? consistencyToken)
    {
        throw new NotImplementedException();
    }

    public Task<PaginatedListDTO<UsuarioListadoDTO>> FindUsuariosBloqueados(string dominioId, string? filtro, string? cursor, int? limit, string? consistencyToken)
    {
        throw new NotImplementedException();
    }

    public Task<List<DominioDetalhesDTO>> GetDominioDetalhes(IEnumerable<string> dominioIds, bool noUsuarioAdministrador, string? consistencyToken)
    {
        throw new NotImplementedException();
    }
}
