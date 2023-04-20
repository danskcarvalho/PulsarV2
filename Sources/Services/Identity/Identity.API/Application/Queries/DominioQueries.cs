using Pulsar.BuildingBlocks.Utils.Bson;
using Pulsar.Services.Shared.DTOs;

namespace Pulsar.Services.Identity.API.Application.Queries;

public partial class DominioQueries : IdentityQueries, IDominioQueries
{
    public DominioQueries(IdentityQueriesContext ctx) : base(ctx)
    {
    }

    public async Task<PaginatedListDTO<DominioListadoDTO>> FindDominios(string? filtro, bool? showHidden, string? cursor, int? limit, string? consistencyToken)
    {
        return await this.StartCausallyConsistentSectionAsync(async ct =>
        {
            var (dominios, next) = 
                await DominiosCollection.Paginated(limit ?? 50, cursor, new { Filtro = filtro, ShowHidden = showHidden })
                    .FindAsync<CursorDominioListado>(c => 
                        c.ShowHidden == true ? 
                            c.Filtro.ToTextSearch<Dominio>() : 
                            Filters.Dominios.Create(f => f.And(c.Filtro.ToTextSearch<Dominio>(), f.Eq(d => d.AuditInfo.EscondidoEm, null))));

            var usuarioIds = dominios.Where(x => x.UsuarioAdministradorId.HasValue).Select(x => x.UsuarioAdministradorId!.Value).ToList();
            var usuarios = (await UsuariosCollection.FindAsync(x => usuarioIds.Contains(x.Id)).ToListAsync()).MapByUnique(x => x.Id);
            var dominiosListados = dominios.Select(x =>
            {
                var usuario = x.UsuarioAdministradorId.HasValue ? usuarios[x.UsuarioAdministradorId.Value] : null;
                return new DominioListadoDTO(x.Id.ToString(), x.Nome, x.IsAtivo, x.AuditInfo.EscondidoEm.HasValue, usuario != null ? new DominioListadoDTO.UsuarioAdmin(usuario.Id.ToString(), usuario.NomeCompleto, usuario.Email!, usuario.NomeUsuario, usuario.AvatarUrl) : null);
            }).ToList();

            return new PaginatedListDTO<DominioListadoDTO>(dominiosListados, next);
        }, consistencyToken);
    }

    public async Task<PaginatedListDTO<UsuarioListadoDTO>> FindUsuariosBloqueados(string dominioId, string? filtro, string? cursor, int? limit, string? consistencyToken)
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

            var (usuarios, next) = await UsuariosCollection.Paginated(limit ?? 50, cursor, new { DominioId = dominioId, Filtro = filtro }).FindAsync<CursorUsuariosBloqueados, UsuarioListadoDTO>(projection,
                c =>
                {
                    var textSearch = !IsEmail(c.Filtro) ? c.Filtro.ToTextSearch<Usuario>() : Filters.Usuarios.Create(f => f.Eq(u => u.Email, c.Filtro) );
                    return Filters.Usuarios.Create(f => f.And(textSearch, f.AnyEq(u => u.DominiosBloqueados, c.DominioId.ToObjectId()), f.Ne(f => f.Email, null)));
                });

            return new PaginatedListDTO<UsuarioListadoDTO>(usuarios, next);
        }, consistencyToken);

        bool IsEmail(string? filtro)
        {
            return filtro?.Contains('@') == true;
        }
    }

    public async Task<List<DominioDetalhesDTO>> GetDominioDetalhes(IEnumerable<string> dominioIds, bool noUsuarioAdministrador, string? consistencyToken)
    {
        if (dominioIds.Count() == 0)
            return new List<DominioDetalhesDTO>();

        var r = await this.StartCausallyConsistentSectionAsync(async ct =>
        {
            var allIds = dominioIds.Select(x => x.ToObjectId()).ToList();
            var dominios = await DominiosCollection.FindAsync(d => allIds.Contains(d.Id)).ToListAsync();
            var allUsuarioIds = dominios.Where(d => d.UsuarioAdministradorId.HasValue).Select(d => d.UsuarioAdministradorId!.Value);
            var usuarios = noUsuarioAdministrador ? null : (await UsuariosCollection.FindAsync(u => allUsuarioIds.Contains(u.Id)).ToListAsync()).MapByUnique(u => u.Id);
            return dominios.Select(d =>
            {
                var usuario = d.UsuarioAdministradorId.HasValue && !noUsuarioAdministrador ? usuarios![d.UsuarioAdministradorId.Value] : null;
                return new DominioDetalhesDTO(d.Id.ToString(), d.Nome, d.IsAtivo, d.AuditInfo.EscondidoEm.HasValue, usuario != null ? new DominioDetalhesDTO.UsuarioAdmin(usuario.Id.ToString(), usuario.NomeCompleto, usuario.Email!, usuario.NomeUsuario) : null);
            });
        }, consistencyToken);
        return r.ToList();
    }
}
