using Pulsar.BuildingBlocks.Utils.Bson;
using Pulsar.Services.Identity.Domain.Aggregates.Grupos;
using Pulsar.Services.Shared.DTOs;

namespace Pulsar.Services.Identity.API.Application.Queries;

public partial class GrupoQueries
{
    private async Task<PaginatedListDTO<UsuarioGrupoListadoDTO>> FindUsuariosByCursor(string dominioId, string strCursor, int limit)
    {
        limit = limit.Limit();
        var cursor = strCursor.FromBase64Json<CursorUsuarioGrupoListadoDTO>()!;
        var grupoId = cursor.GrupoId.ToObjectId();
        var grupo = await (await GruposCollection.FindAsync(g => g.Id == grupoId)).FirstOrDefaultAsync();
        if (grupo == null || grupo.DominioId != dominioId.ToObjectId())
            return new PaginatedListDTO<UsuarioGrupoListadoDTO>(new List<UsuarioGrupoListadoDTO>(), null);
        var subgrupos = new Dictionary<ObjectId, SubGrupo>();
        foreach (var sg in grupo.SubGrupos)
            subgrupos[sg.SubGrupoId] = sg;

        //TODO: Add Indexes
        var textSearch = !IsEmail(cursor.Filtro) ? cursor.Filtro.ToTextSearch() : BSON.Create(b => new { Email = b.Eq(cursor.Filtro) });
        var filter = BSON.Create(b => b.And(textSearch, new Dictionary<string, object> { ["Grupos.GrupoId"] = b.Eq(cursor.GrupoId.ToObjectId()) }, new { Email = b.Ne(null) }, new { Email = b.Gt(cursor.LastEmail) }));

        var findOptions = new FindOptions<Usuario, Usuario>()
        {
            Sort = Builders<Usuario>.Sort.Ascending(x => x.Email),
            Limit = limit
        };
        var usuarios = (await (await UsuariosCollection.FindAsync(filter, findOptions)).ToListAsync()).Select(x => new UsuarioGrupoListadoDTO(x.Id.ToString(), x.Email!, x.PrimeiroNome, x.NomeCompleto, x.NomeUsuario)
        {
            AvatarUrl = x.Avatar == null ? null : x.Avatar.PublicUrl,
            IsAtivo = x.IsAtivo,
            Email = x.Email!,
            IsConvitePendente = x.IsConvitePendente,
            UltimoNome = x.UltimoNome,
            SubGrupos = GetSubGrupos(x, subgrupos)
        }).ToList();
        return new PaginatedListDTO<UsuarioGrupoListadoDTO>(usuarios, cursor.Next(usuarios)?.ToBase64Json());
    }

    private static List<UsuarioGrupoListadoDTO.SubGrupoDetalhes> GetSubGrupos(Usuario x, Dictionary<ObjectId, SubGrupo> subgrupos)
    {
        return x.Grupos.Where(y => subgrupos.ContainsKey(y.SubGrupoId)).Select(y => new UsuarioGrupoListadoDTO.SubGrupoDetalhes(y.SubGrupoId.ToString(), subgrupos[y.SubGrupoId].Nome)).OrderBy(y => y.Nome).ToList();
    }

    private async Task<PaginatedListDTO<UsuarioGrupoListadoDTO>> FindUsuariosWithoutCursor(string dominioId, string? filtro, string grupoId, int limit)
    {
        limit = limit.Limit();
        var grupo = await (await GruposCollection.FindAsync(g => g.Id == grupoId.ToObjectId())).FirstOrDefaultAsync();
        if (grupo == null || grupo.DominioId != dominioId.ToObjectId())
            return new PaginatedListDTO<UsuarioGrupoListadoDTO>(new List<UsuarioGrupoListadoDTO>(), null);
        var subgrupos = new Dictionary<ObjectId, SubGrupo>();
        foreach (var sg in grupo.SubGrupos)
            subgrupos[sg.SubGrupoId] = sg;

        //TODO: Add Indexes
        var textSearch = !IsEmail(filtro) ? filtro.ToTextSearch() : BSON.Create(b => new { Email = b.Eq(filtro) });
        var filter = BSON.Create(b => b.And(textSearch, new Dictionary<string, object> { ["Grupos.GrupoId"] = b.Eq(grupoId.ToObjectId()) }, new { Email = b.Ne(null) }));

        var findOptions = new FindOptions<Usuario, Usuario>()
        {
            Sort = Builders<Usuario>.Sort.Ascending(x => x.Email),
            Limit = limit
        };
        var usuarios = (await (await UsuariosCollection.FindAsync(filter, findOptions)).ToListAsync()).Select(x => new UsuarioGrupoListadoDTO(x.Id.ToString(), x.Email!, x.PrimeiroNome, x.NomeCompleto, x.NomeUsuario)
        {
            AvatarUrl = x.Avatar == null ? null : x.Avatar.PublicUrl,
            IsAtivo = x.IsAtivo,
            Email = x.Email!,
            IsConvitePendente = x.IsConvitePendente,
            UltimoNome = x.UltimoNome,
            SubGrupos = GetSubGrupos(x, subgrupos)
        }).ToList();
        return new PaginatedListDTO<UsuarioGrupoListadoDTO>(usuarios, CursorUsuarioGrupoListadoDTO.Next(usuarios, dominioId, filtro)?.ToBase64Json());
    }

    private bool IsEmail(string? filtro)
    {
        return filtro?.Contains('@') == true;
    }
}
