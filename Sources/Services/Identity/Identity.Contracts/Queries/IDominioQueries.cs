using Pulsar.Services.Shared.DTOs;

namespace Pulsar.Services.Identity.Contracts.Queries;

public interface IDominioQueries
{
    Task<PaginatedListDTO<DominioListadoDTO>> FindDominios(string? filtro, string? cursor, int? limit, string? consistencyToken);
    Task<List<DominioDetalhesDTO>> GetDominioDetalhes(IEnumerable<string> dominioIds, string? consistencyToken);
    Task<PaginatedListDTO<UsuarioListadoDTO>> FindUsuariosBloqueados(string dominioId, string? filtro, string? cursor, int? limit, string? consistencyToken);
}
