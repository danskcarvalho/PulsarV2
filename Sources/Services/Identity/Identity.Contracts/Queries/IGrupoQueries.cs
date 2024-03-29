﻿using Pulsar.Services.Shared.DTOs;

namespace Pulsar.Services.Identity.Contracts.Queries;

public interface IGrupoQueries
{
    Task<PaginatedListDTO<GrupoListadoDTO>> FindGrupos(string dominioId, string? filtro, string? cursor, int? limit, string? consistencyToken);
    Task<List<GrupoDetalhesDTO>> GetGrupoDetalhes(string dominioId, IEnumerable<string> grupoIds, string? consistencyToken);
    Task<PaginatedListDTO<UsuarioListadoDTO>> FindUsuarios(string dominioId, string grupoId, string subgrupoId, string? filtro, string? cursor, int? limit, string? consistencyToken);
}
