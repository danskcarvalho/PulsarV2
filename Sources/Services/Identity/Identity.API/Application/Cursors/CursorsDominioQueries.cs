using Pulsar.BuildingBlocks.DDD.Mongo.Cursors;
using Pulsar.Services.Identity.Domain.Aggregates.Usuarios;
using System.Diagnostics.CodeAnalysis;

namespace Pulsar.Services.Identity.API.Application.Cursors;

public class CursorDominioListado : IPageCursor<CursorDominioListado, Dominio>
{
    public required string? LastNome { get; set; }
    public required string? LastId { get; set; }
    public string? Filtro { get; set; }
    public bool? ShowHidden { get; set; }

    public (string Name, object? Value) SortColumn1 => 
        ("Nome", LastNome);

    public (string Name, object? Value)? SortColumn2 =>
        ("Id", LastId?.ToObjectId());

    public static bool HasSortColumn2 => true;

    [SetsRequiredMembers]
    public CursorDominioListado(string? lastNome, string? lastId, string? filtro, bool? showHidden)
    {
        LastNome = lastNome;
        LastId = lastId;
        Filtro = filtro;
        ShowHidden = showHidden;
    }
    private CursorDominioListado() { }

    public static CursorDominioListado FromFilter(dynamic filter)
    {
        return new CursorDominioListado(null, null, filter.Filtro, filter.ShowHidden);
    }

    public CursorDominioListado Next(Dominio last)
    {
        return new CursorDominioListado(last.Nome, last.Id.ToString(), this.Filtro, this.ShowHidden);
    }
}

public class CursorUsuariosBloqueados : IPageCursor<CursorUsuariosBloqueados, UsuarioListadoDTO>
{
    public required string? LastEmail { get; set; }
    public required string DominioId { get; set; }
    public string? Filtro { get; set; }

    public static bool HasSortColumn2 => false;

    public (string Name, object? Value) SortColumn1 => ("Email", LastEmail);

    public (string Name, object? Value)? SortColumn2 => null;

    [SetsRequiredMembers]
    public CursorUsuariosBloqueados(string lastEmail, string dominioId, string? filtro)
    {
        LastEmail = lastEmail;
        DominioId = dominioId;
        Filtro = filtro;
    }
    private CursorUsuariosBloqueados() { }

    public static CursorUsuariosBloqueados FromFilter(dynamic filter)
    {
        return new CursorUsuariosBloqueados(null, filter.DominioId, filter.Filtro);
    }

    public CursorUsuariosBloqueados Next(UsuarioListadoDTO last)
    {
        return new CursorUsuariosBloqueados(last.Email, this.DominioId, this.Filtro);
    }
}
