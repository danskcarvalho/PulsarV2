using Pipelines.Sockets.Unofficial.Arenas;
using Pulsar.BuildingBlocks.Utils.Bson;
using Pulsar.Services.Identity.Domain.Aggregates.Grupos;
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
            var (grupos, next) = await GruposCollection.Paginated(limit ?? 50, cursor, new { Filtro = filtro }).FindAsync<CursorGrupoListado>(
                 c =>
                 {
                     var textSearch = c.Filtro.ToTextSearch<Grupo>();
                     return Filters.Grupos.Create(f => f.And(
                         textSearch,
                         f.Eq(g => g.DominioId, dominioId.ToObjectId()),
                         f.Eq(g => g.AuditInfo.RemovidoEm, null)));
                 });

            var gruposListados = grupos.Select(x => new GrupoListadoDTO(x.Id.ToString(), x.Nome, x.NumSubGrupos, x.NumUsuarios)).ToList();
            return new PaginatedListDTO<GrupoListadoDTO>(gruposListados, next);
        }, consistencyToken);
    }

    public async Task<PaginatedListDTO<UsuarioListadoDTO>> FindUsuarios(string dominioId, string grupoId, string subgrupoId, string? filtro, string? cursor, int? limit, string? consistencyToken)
    {
        filtro = filtro?.ToLowerInvariant().Trim();

        return await this.StartCausallyConsistentSectionAsync(async ct =>
        {
            var projection = Builders<Usuario>.Projection.Expression(x => new UsuarioListadoDTO(x.Id.ToString(), x.Email!, x.PrimeiroNome, x.NomeCompleto, x.NomeUsuario)
            {
                AvatarUrl = x.AvatarUrl,
                IsAtivo = x.IsAtivo,
                IsConvitePendente = x.IsConvitePendente,
                UltimoNome = x.UltimoNome
            });

            var (usuarios, next) = await UsuariosCollection.Paginated(limit ?? 50, cursor, new { GrupoId = grupoId, SubGrupoId = subgrupoId, Filtro = filtro }).FindAsyncWithAsyncFilter<CursorUsuarioGrupoListado, UsuarioListadoDTO>(projection,
                 async c =>
                 {
                     var grupoId = c.GrupoId.ToObjectId();
                     var subgrupoId = c.SubGrupoId.ToObjectId();
                     var grupo = await GruposCollection.FindAsync(g => g.Id == grupoId).FirstOrDefaultAsync();
                     if (grupo == null || grupo.DominioId != dominioId.ToObjectId() || !grupo.SubGrupos.Any(sg => sg.SubGrupoId == subgrupoId))
                         return null; // return nothing
                     var subgrupo = grupo.SubGrupos.FirstOrDefault(sg => sg.SubGrupoId == subgrupoId);
                     if (subgrupo == null)
                         return null;

                     var textSearch = !IsEmail(c.Filtro) ? c.Filtro.ToTextSearch<Usuario>() : Filters.Usuarios.Create(f => f.Eq(u => u.Email, c.Filtro));
                     return Filters.Usuarios.Create(f => f.And(
                         textSearch,
                         f.In(u => u.Id, subgrupo.UsuarioIds),
                         f.Ne(u => u.Email, null)));
                 });
            return new PaginatedListDTO<UsuarioListadoDTO>(usuarios, next);
        }, consistencyToken);

        bool IsEmail(string? filtro)
        {
            return filtro?.Contains('@') == true;
        }
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
                                return redeEstabelecimentos.ContainsKey(pe.Seletor.RedeEstabelecimentoId!.Value) && redeEstabelecimentos[pe.Seletor.RedeEstabelecimentoId!.Value].AuditInfo.RemovidoEm == null;
                        })
                        .Select(pe => new GrupoDetalhesDTO.SubGrupoPermissoesEstabelecimentoDetalhes(
                            pe.Seletor.EstabelecimentoId?.ToString(),
                            pe.Seletor.EstabelecimentoId != null ? estabelecimentos[pe.Seletor.EstabelecimentoId.Value].Nome : null,
                            pe.Seletor.EstabelecimentoId != null ? estabelecimentos[pe.Seletor.EstabelecimentoId.Value].Cnes : null,
                            pe.Seletor.RedeEstabelecimentoId?.ToString(),
                            pe.Seletor.RedeEstabelecimentoId != null ? redeEstabelecimentos[pe.Seletor.RedeEstabelecimentoId.Value].Nome : null,
                            pe.Permissoes.ToList())).ToList())).ToList());
                return dto;
            }).ToList();
        }, consistencyToken);
    }
}
