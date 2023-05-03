using Pulsar.Services.Identity.Contracts.DTOs;
using Pulsar.Services.Identity.Contracts.Queries;
using Pulsar.Services.Shared.DTOs;

namespace Identity.UnitTests.Queries;

public class MockedGrupoQueries : IGrupoQueries
{
    public Task<PaginatedListDTO<GrupoListadoDTO>> FindGrupos(string dominioId, string? filtro, string? cursor, int? limit, string? consistencyToken)
    {
        throw new NotImplementedException();
    }

    public Task<PaginatedListDTO<UsuarioListadoDTO>> FindUsuarios(string dominioId, string grupoId, string subgrupoId, string? filtro, string? cursor, int? limit, string? consistencyToken)
    {
        throw new NotImplementedException();
    }

    public Task<List<GrupoDetalhesDTO>> GetGrupoDetalhes(string dominioId, IEnumerable<string> grupoIds, string? consistencyToken)
    {
        throw new NotImplementedException();
    }
}
