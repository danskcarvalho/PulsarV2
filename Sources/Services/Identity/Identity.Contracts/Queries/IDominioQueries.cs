using Pulsar.Services.Shared.DTOs;

namespace Pulsar.Services.Identity.Contracts.Queries;

public interface IDominioQueries
{
    Task<PaginatedListDTO<DominioListadoDTO>> FindDominios(string? filtro, bool? showHidden, string? cursor, int? limit, string? consistencyToken);
    Task<List<DominioDetalhesDTO>> GetDominioDetalhes(IEnumerable<string> dominioIds, bool noUsuarioAdministrador, string? consistencyToken);
    Task<PaginatedListDTO<UsuarioListadoDTO>> FindUsuariosBloqueados(string dominioId, string? filtro, string? cursor, int? limit, string? consistencyToken);
}
