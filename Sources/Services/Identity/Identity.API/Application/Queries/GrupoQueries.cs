using Pulsar.Services.Shared.DTOs;

namespace Pulsar.Services.Identity.API.Application.Queries;

public partial class GrupoQueries : IdentityQueries, IGrupoQueries
{
    public GrupoQueries(IdentityQueriesContext ctx) : base(ctx)
    {
    }

    public async Task<PaginatedListDTO<GrupoListadoDTO>> FindGrupos(string dominioId, string? filtro, string? cursor, int? limit, string? consistencyToken)
    {
        return await this.StartCausallyConsistentSectionAsync(async ct =>
        {
            if (cursor is null)
                return await FindGruposWithoutCursor(dominioId, filtro, limit ?? 50);
            else
                return await FindGruposByCursor(dominioId, cursor, limit ?? 50);
        }, consistencyToken);
    }

    public async Task<PaginatedListDTO<UsuarioListadoDTO>> FindUsuarios(string dominioId, string grupoId, string subgrupoId, string? filtro, string? cursor, int? limit, string? consistencyToken)
    {
        return await this.StartCausallyConsistentSectionAsync(async ct =>
        {
            if (cursor is null)
                return await FindUsuariosWithoutCursor(dominioId, filtro, grupoId, subgrupoId, limit ?? 50);
            else
                return await FindUsuariosByCursor(dominioId, cursor, limit ?? 50);
        }, consistencyToken);
    }

    public async Task<List<GrupoDetalhesDTO>> GetGrupoDetalhes(string dominioId, IEnumerable<string> grupoIds, string? consistencyToken)
    {
        if (grupoIds.Count() == 0)
            return new List<GrupoDetalhesDTO>();
        return await this.StartCausallyConsistentSectionAsync(async ct =>
        {
            var allIds = grupoIds.Select(x => x.ToObjectId()).ToList();
            var grupos = await GruposCollection.FindAsync(g => allIds.Contains(g.Id) && g.DominioId == dominioId.ToObjectId()).ToListAsync();
            var estabelecimentosIds = grupos.SelectMany(g => g.SubGrupos).SelectMany(sg => sg.PermissoesEstabelecimentos).Where(pe => pe.Seletor.EstabelecimentoId.HasValue)
                .Select(pe => pe.Seletor.EstabelecimentoId!.Value).ToList();
            var redeEstabelecimentosIds = grupos.SelectMany(g => g.SubGrupos).SelectMany(sg => sg.PermissoesEstabelecimentos).Where(pe => pe.Seletor.RedeEstabelecimentoId.HasValue)
                .Select(pe => pe.Seletor.RedeEstabelecimentoId!.Value).ToList();
            var estabelecimentos = (await EstabelecimentosCollection.FindAsync(e => estabelecimentosIds.Contains(e.Id)).ToListAsync()).MapByUnique(e => e.Id);
            var redeEstabelecimentos = (await RedesEstabelecimentosCollection.FindAsync(re => redeEstabelecimentosIds.Contains(re.Id)).ToListAsync()).MapByUnique(e => e.Id);

            return grupos.Select(g =>
            {
                var dto = new GrupoDetalhesDTO(g.Id.ToString(), g.Nome, g.NumSubGrupos, g.NumUsuarios,
                    g.SubGrupos.Select(sg => new GrupoDetalhesDTO.SubGrupoDetalhes(sg.SubGrupoId.ToString(), sg.Nome, sg.PermissoesDominio.ToList(), sg.NumUsuarios,
                        sg.PermissoesEstabelecimentos
                        .Where(pe =>
                        {
                            if (pe.Seletor.EstabelecimentoId.HasValue)
                                return estabelecimentos.ContainsKey(pe.Seletor.EstabelecimentoId.Value);
                            else
                                return redeEstabelecimentos.ContainsKey(pe.Seletor.RedeEstabelecimentoId!.Value);
                        })
                        .Select(pe => new GrupoDetalhesDTO.SubGrupoPermissoesEstabelecimentoDetalhes(
                            pe.Seletor.EstabelecimentoId?.ToString(),
                            pe.Seletor.EstabelecimentoId != null ? estabelecimentos[pe.Seletor.EstabelecimentoId.Value].Nome : null,
                            pe.Seletor.EstabelecimentoId != null ? estabelecimentos[pe.Seletor.EstabelecimentoId.Value].Cnes : null,
                            pe.Seletor.RedeEstabelecimentoId?.ToString(),
                            pe.Seletor.RedeEstabelecimentoId != null ? estabelecimentos[pe.Seletor.RedeEstabelecimentoId.Value].Nome : null,
                            pe.Permissoes.ToList())).ToList())).ToList());
                return dto;
            }).ToList();
        }, consistencyToken);
    }

    public static class CacheCategories
    {
        public const string FindGrupos = "GrupoQueries.FindGrupos";
    }
}
