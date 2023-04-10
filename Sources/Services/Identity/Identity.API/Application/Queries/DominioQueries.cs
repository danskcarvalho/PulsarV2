using Pulsar.Services.Shared.DTOs;

namespace Pulsar.Services.Identity.API.Application.Queries;

public partial class DominioQueries : IdentityQueries, IDominioQueries
{
    public DominioQueries(IdentityQueriesContext ctx) : base(ctx)
    {
    }

    public async Task<PaginatedListDTO<DominioListadoDTO>> FindDominios(string? filtro, string? cursor, int? limit, string? consistencyToken)
    {
        return await this.StartCausallyConsistentSectionAsync(async ct =>
        {
            if (cursor is null)
                return await FindDominiosWithoutCursor(filtro, limit ?? 50);
            else
                return await FindDominiosByCursor(cursor, limit ?? 50);
        }, consistencyToken);
    }

    public async Task<PaginatedListDTO<UsuarioListadoDTO>> FindUsuariosBloqueados(string dominioId, string? filtro, string? cursor, int? limit, string? consistencyToken)
    {
        return await this.StartCausallyConsistentSectionAsync(async ct =>
        {
            if (cursor is null)
                return await FindUsuariosBloqueadosWithoutCursor(filtro, dominioId, limit ?? 50);
            else
                return await FindUsuariosBloqueadosByCursor(cursor, limit ?? 50);
        }, consistencyToken);
    }

    public async Task<List<DominioDetalhesDTO>> GetDominioDetalhes(IEnumerable<string> dominioIds, string? consistencyToken)
    {
        if (dominioIds.Count() == 0)
            return new List<DominioDetalhesDTO>();

        var r = await this.StartCausallyConsistentSectionAsync(async ct =>
        {
            var allIds = dominioIds.Select(x => x.ToObjectId()).ToList();
            var dominios = await (await DominiosCollection.FindAsync(d => allIds.Contains(d.Id))).ToListAsync();
            var allUsuarioIds = dominios.Where(d => d.UsuarioAdministradorId.HasValue).Select(d => d.UsuarioAdministradorId!.Value);
            var usuarios = (await (await UsuariosCollection.FindAsync(u => allUsuarioIds.Contains(u.Id))).ToListAsync()).MapByUnique(u => u.Id);
            return dominios.Select(d =>
            {
                var usuario = d.UsuarioAdministradorId.HasValue ? usuarios[d.UsuarioAdministradorId.Value] : null;
                return new DominioDetalhesDTO(d.Id.ToString(), d.Nome, d.IsAtivo, usuario != null ? new DominioDetalhesDTO.UsuarioAdmin(usuario.Id.ToString(), usuario.NomeCompleto, usuario.Email!, usuario.NomeUsuario) : null);
            });
        }, consistencyToken);
        return r.ToList();
    }
}
