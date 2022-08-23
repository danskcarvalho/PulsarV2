using Pulsar.BuildingBlocks.Utils.Bson;
using Pulsar.Services.Shared.DTOs;

namespace Pulsar.Services.Identity.API.Application.Queries;

public partial class UsuarioQueries
{
    private async Task<PaginatedListDTO<UsuarioListadoDTO>> FindUsuariosByCursor(string strCursor, int limit)
    {
        limit = limit.Limit();
        var cursor = strCursor.FromBase64Json<CursorUsuarioListadoDTO>()!;
        var textSearch = !IsEmail(cursor.Filtro) ? cursor.Filtro.ToTextSearch() : BSON.Create(b => new { Email = b.Eq(cursor.Filtro) });
        var filter = BSON.Create(b => b.And(textSearch, new { Email = b.Ne(null) }, new { Email = b.Gt(cursor.LastEmail) }));

        var findOptions = new FindOptions<Usuario, UsuarioListadoDTO>()
        {
            Projection = Builders<Usuario>.Projection.Expression(x => new UsuarioListadoDTO(x.Id.ToString(), x.Email!, x.PrimeiroNome, x.NomeCompleto, x.NomeUsuario)
            {
                AvatarUrl = x.Avatar == null ? null : x.Avatar.PublicUrl,
                IsAtivo = x.IsAtivo,
                Email = x.Email!,
                IsConvitePendente = x.IsConvitePendente,
                UltimoNome = x.UltimoNome
            }),
            Sort = Builders<Usuario>.Sort.Ascending(x => x.Email),
            Limit = limit
        };
        var usuarios = await (await Usuarios.FindAsync(filter, findOptions)).ToListAsync();
        return new PaginatedListDTO<UsuarioListadoDTO>(usuarios, cursor.Next(usuarios)?.ToBase64Json());
    }

    private async Task<PaginatedListDTO<UsuarioListadoDTO>> FindUsuariosWithoutCursor(string? filtro, int limit)
    {
        limit = limit.Limit();
        var textSearch = !IsEmail(filtro) ? filtro.ToTextSearch() : BSON.Create(b => new { Email = b.Eq(filtro) });
        var filter = BSON.Create(b => b.And(textSearch, new { Email = b.Ne(null) }));

        var findOptions = new FindOptions<Usuario, UsuarioListadoDTO>()
        {
            Projection = Builders<Usuario>.Projection.Expression(x => new UsuarioListadoDTO(x.Id.ToString(), x.Email!, x.PrimeiroNome, x.NomeCompleto, x.NomeUsuario)
            {
                AvatarUrl = x.Avatar == null ? null : x.Avatar.PublicUrl,
                IsAtivo = x.IsAtivo,
                Email = x.Email!,
                IsConvitePendente = x.IsConvitePendente,
                UltimoNome = x.UltimoNome
            }),
            Sort = Builders<Usuario>.Sort.Ascending(x => x.Email),
            Limit = limit
        };
        var usuarios = await (await Usuarios.FindAsync(filter, findOptions)).ToListAsync();
        return new PaginatedListDTO<UsuarioListadoDTO>(usuarios, CursorUsuarioListadoDTO.Next(usuarios, filtro)?.ToBase64Json());
    }

    private bool IsEmail(string? filtro)
    {
        return filtro?.Contains('@') == true;
    }
}
