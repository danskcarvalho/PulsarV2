using Pulsar.BuildingBlocks.Utils.Bson;
using Pulsar.Services.Identity.Domain.Aggregates.Usuarios;
using Pulsar.Services.Shared.DTOs;

namespace Pulsar.Services.Identity.API.Application.Queries;

public partial class DominioQueries
{
    private async Task<PaginatedListDTO<DominioListadoDTO>> FindDominiosByCursor(string strCursor, int limit)
    {
        limit = limit.Limit();
        var cursor = strCursor.FromBase64Json<CursorDominioListadoDTO>()!;
        var textSearch = cursor.Filtro.ToTextSearch();
        var filter = BSON.Create(b => b.And(
            textSearch, 
            b.Or(new { Nome = b.Gt(cursor.LastNome) }, b.And(new { Nome = b.Eq(cursor.LastNome) }, new { Id = b.Gt(cursor.LastDominioId.ToObjectId()) }))));

        var findOptions = new FindOptions<Dominio, Dominio>()
        {
            Sort = Builders<Dominio>.Sort.Ascending(x => x.Nome).Ascending(x => x.Id),
            Limit = limit
        };
        var dominios = await (await DominiosCollection.FindAsync(filter, findOptions)).ToListAsync();
        var usuarioIds = dominios.Where(x => x.UsuarioAdministradorId.HasValue).Select(x => x.UsuarioAdministradorId!.Value).ToList();
        var usuarios = (await (await UsuariosCollection.FindAsync(x => usuarioIds.Contains(x.Id))).ToListAsync()).MapByUnique(x => x.Id);
        var dominiosListados = dominios.Select(x =>
        {
            var usuario = x.UsuarioAdministradorId.HasValue ? usuarios[x.UsuarioAdministradorId.Value] : null;
            return new DominioListadoDTO(x.Id.ToString(), x.Nome, x.IsAtivo, usuario != null ? new DominioListadoDTO.UsuarioAdmin(usuario.Id.ToString(), usuario.NomeCompleto, usuario.Email!, usuario.NomeUsuario) : null);
        }).ToList();

        return new PaginatedListDTO<DominioListadoDTO>(dominiosListados, cursor.Next(dominiosListados)?.ToBase64Json());
    }

    private async Task<PaginatedListDTO<DominioListadoDTO>> FindDominiosWithoutCursor(string? filtro, int limit)
    {
        limit = limit.Limit();
        var filter = filtro.ToTextSearch();

        var findOptions = new FindOptions<Dominio, Dominio>()
        {
            Sort = Builders<Dominio>.Sort.Ascending(x => x.Nome).Ascending(x => x.Id),
            Limit = limit
        };
        var dominios = await (await DominiosCollection.FindAsync(filter, findOptions)).ToListAsync();
        var usuarioIds = dominios.Where(x => x.UsuarioAdministradorId.HasValue).Select(x => x.UsuarioAdministradorId!.Value).ToList();
        var usuarios = (await (await UsuariosCollection.FindAsync(x => usuarioIds.Contains(x.Id))).ToListAsync()).MapByUnique(x => x.Id);
        var dominiosListados = dominios.Select(x =>
        {
            var usuario = x.UsuarioAdministradorId.HasValue ? usuarios[x.UsuarioAdministradorId.Value] : null;
            return new DominioListadoDTO(x.Id.ToString(), x.Nome, x.IsAtivo, usuario != null ? new DominioListadoDTO.UsuarioAdmin(usuario.Id.ToString(), usuario.NomeCompleto, usuario.Email!, usuario.NomeUsuario) : null);
        }).ToList();

        return new PaginatedListDTO<DominioListadoDTO>(dominiosListados, CursorDominioListadoDTO.Next(dominiosListados, filtro)?.ToBase64Json());
    }

}
