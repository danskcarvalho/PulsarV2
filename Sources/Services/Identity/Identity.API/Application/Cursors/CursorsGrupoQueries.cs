using Pulsar.BuildingBlocks.DDD.Mongo.Cursors;
using System.Diagnostics.CodeAnalysis;

namespace Pulsar.Services.Identity.API.Application.Cursors;

public class CursorGrupoListado : IPageCursor<CursorGrupoListado, Grupo>
{
    public required string? LastNome { get; set; }
    public required string? LastGrupoId { get; set; }
    public string? Filtro { get; set; }

    public static bool HasSortColumn2 => true;

    public (string Name, object? Value) SortColumn1 => ("Nome", LastNome);

    public (string Name, object? Value)? SortColumn2 => ("Id", LastGrupoId?.ToObjectId());

    [SetsRequiredMembers]
    public CursorGrupoListado(string? lastNome, string? lastGrupoId, string? filtro)
    {
        LastNome = lastNome;
        LastGrupoId = lastGrupoId;
        Filtro = filtro;
    }
    private CursorGrupoListado() { }

    public static CursorGrupoListado FromFilter(dynamic filter)
    {
        return new CursorGrupoListado(null, null, filter.Filtro);
    }

    public CursorGrupoListado Next(Grupo last)
    {
        return new CursorGrupoListado(last.Nome, last.Id.ToString(), this.Filtro);
    }
}

public class CursorUsuarioGrupoListado : IPageCursor<CursorUsuarioGrupoListado, UsuarioListadoDTO>
{
    public required string? LastEmail { get; set; }
    public required string GrupoId { get; set; }
    public required string SubGrupoId { get; set; }
    public string? Filtro { get; set; }

    public static bool HasSortColumn2 => false;

    public (string Name, object? Value) SortColumn1 => ("Email", LastEmail);

    public (string Name, object? Value)? SortColumn2 => null;

    [SetsRequiredMembers]
    public CursorUsuarioGrupoListado(string? lastEmail, string grupoId, string subGrupoId, string? filtro)
    {
        LastEmail = lastEmail;
        GrupoId = grupoId;
        SubGrupoId = subGrupoId;
        Filtro = filtro;
    }

    private CursorUsuarioGrupoListado() { }

    public static CursorUsuarioGrupoListado FromFilter(dynamic filter)
    {
        return new CursorUsuarioGrupoListado(null, filter.GrupoId, filter.SubGrupoId, filter.Filtro);
    }

    public CursorUsuarioGrupoListado Next(UsuarioListadoDTO last)
    {
        return new CursorUsuarioGrupoListado(last.Email, this.GrupoId, this.SubGrupoId, this.Filtro);
    }
}
