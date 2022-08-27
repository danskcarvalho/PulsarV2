using Pulsar.BuildingBlocks.Utils.Bson;
using Pulsar.Services.Identity.Domain.Aggregates.Grupos;
using Pulsar.Services.Shared.DTOs;

namespace Pulsar.Services.Identity.API.Application.Queries;

public partial class GrupoQueries
{
    private async Task<PaginatedListDTO<UsuarioListadoDTO>> FindUsuariosByCursor(string dominioId, string strCursor, int limit)
    {
        limit = limit.Limit();
        var cursor = strCursor.FromBase64Json<CursorUsuarioGrupoListadoDTO>()!;
        var grupoId = cursor.GrupoId.ToObjectId();
        var grupo = await (await GruposCollection.FindAsync(g => g.Id == grupoId)).FirstOrDefaultAsync();
        if (grupo == null || grupo.DominioId != dominioId.ToObjectId() || !grupo.SubGrupos.Any(sg => sg.SubGrupoId == cursor.SubGrupoId.ToObjectId()))
            return new PaginatedListDTO<UsuarioListadoDTO>(new List<UsuarioListadoDTO>(), null);
        var subgrupos = new Dictionary<ObjectId, SubGrupo>();
        foreach (var sg in grupo.SubGrupos)
            subgrupos[sg.SubGrupoId] = sg;

        var textSearch = !IsEmail(cursor.Filtro) ? cursor.Filtro.ToTextSearch() : BSON.Create(b => new { Email = b.Eq(cursor.Filtro) });
        var filter = BSON.Create(b => b.And(
            textSearch, 
            new Dictionary<string, object> { ["Grupos.GrupoId"] = b.Eq(cursor.GrupoId.ToObjectId()) },
            new Dictionary<string, object> { ["Grupos.SubGrupoId"] = b.Eq(cursor.SubGrupoId.ToObjectId()) },
            new { Email = b.Ne(null) }, 
            new { Email = b.Gt(cursor.LastEmail) }));

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
        var usuarios = await (await UsuariosCollection.FindAsync(filter, findOptions)).ToListAsync();
        return new PaginatedListDTO<UsuarioListadoDTO>(usuarios, cursor.Next(usuarios)?.ToBase64Json());
    }

    private async Task<PaginatedListDTO<UsuarioListadoDTO>> FindUsuariosWithoutCursor(string dominioId, string? filtro, string grupoId, string subgrupoId, int limit)
    {
        limit = limit.Limit();
        var grupo = await (await GruposCollection.FindAsync(g => g.Id == grupoId.ToObjectId())).FirstOrDefaultAsync();
        if (grupo == null || grupo.DominioId != dominioId.ToObjectId() || !grupo.SubGrupos.Any(sg => sg.SubGrupoId == subgrupoId.ToObjectId()))
            return new PaginatedListDTO<UsuarioListadoDTO>(new List<UsuarioListadoDTO>(), null);
        var subgrupos = new Dictionary<ObjectId, SubGrupo>();
        foreach (var sg in grupo.SubGrupos)
            subgrupos[sg.SubGrupoId] = sg;

        var textSearch = !IsEmail(filtro) ? filtro.ToTextSearch() : BSON.Create(b => new { Email = b.Eq(filtro) });
        var filter = BSON.Create(b => b.And(
            textSearch, 
            new Dictionary<string, object> { ["Grupos.GrupoId"] = b.Eq(grupoId.ToObjectId()) },
            new Dictionary<string, object> { ["Grupos.SubGrupoId"] = b.Eq(subgrupoId.ToObjectId()) },
            new { Email = b.Ne(null) }));

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
        var usuarios = await (await UsuariosCollection.FindAsync(filter, findOptions)).ToListAsync();
        return new PaginatedListDTO<UsuarioListadoDTO>(usuarios, CursorUsuarioGrupoListadoDTO.Next(usuarios, grupoId, subgrupoId, filtro)?.ToBase64Json());
    }

    private bool IsEmail(string? filtro)
    {
        return filtro?.Contains('@') == true;
    }
}
