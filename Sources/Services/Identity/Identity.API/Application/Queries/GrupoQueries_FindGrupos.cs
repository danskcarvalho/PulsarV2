using Pulsar.BuildingBlocks.Utils.Bson;
using Pulsar.Services.Shared.DTOs;

namespace Pulsar.Services.Identity.API.Application.Queries;

public partial class GrupoQueries
{
    private async Task<PaginatedListDTO<GrupoListadoDTO>> FindGruposByCursor(string dominioId, string strCursor, int limit)
    {
        limit = limit.Limit();
        var cursor = strCursor.FromBase64Json<CursorGrupoListadoDTO>()!;
        var textSearch = cursor.Filtro.ToTextSearch();
        var filter = BSON.Create(b => b.And(
            textSearch,
            new { DominioId = b.Eq(dominioId.ToObjectId()) },
            new Dictionary<string, object> { ["AuditInfo.RemovidoEm"] = b.Eq(null) },
            b.Or(new { Nome = b.Gt(cursor.LastNome) }, b.And(new { Nome = b.Eq(cursor.LastNome) }, new { Id = b.Gt(cursor.LastGrupoId.ToObjectId()) }))));

        var findOptions = new FindOptions<Grupo, Grupo>()
        {
            Sort = Builders<Grupo>.Sort.Ascending(x => x.Nome).Ascending(x => x.Id),
            Limit = limit
        };
        var grupos = await (await GruposCollection.FindAsync(filter, findOptions)).ToListAsync();
        var gruposListados = grupos.Select(x => new GrupoListadoDTO(x.Id.ToString(), x.Nome, x.NumSubGrupos, x.NumUsuarios)).ToList();

        return new PaginatedListDTO<GrupoListadoDTO>(gruposListados, cursor.Next(gruposListados)?.ToBase64Json());
    }

    private async Task<PaginatedListDTO<GrupoListadoDTO>> FindGruposWithoutCursor(string dominioId, string? filtro, int limit)
    {
        limit = limit.Limit();
        var filter = BSON.Create(b => b.And(
            filtro.ToTextSearch(),
            new Dictionary<string, object> { ["AuditInfo.RemovidoEm"] = b.Eq(null) },
            new { DominioId = b.Eq(dominioId.ToObjectId()) }));

        var findOptions = new FindOptions<Grupo, Grupo>()
        {
            Sort = Builders<Grupo>.Sort.Ascending(x => x.Nome).Ascending(x => x.Id),
            Limit = limit
        };
        var grupos = await (await GruposCollection.FindAsync(filter, findOptions)).ToListAsync();
        var gruposListados = grupos.Select(x => new GrupoListadoDTO(x.Id.ToString(), x.Nome, x.NumSubGrupos, x.NumUsuarios)).ToList();

        return new PaginatedListDTO<GrupoListadoDTO>(gruposListados, CursorGrupoListadoDTO.Next(gruposListados, filtro)?.ToBase64Json());
    }
}
