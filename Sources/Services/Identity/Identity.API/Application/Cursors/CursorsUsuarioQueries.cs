using System.Diagnostics.CodeAnalysis;

namespace Pulsar.Services.Identity.API.Application.Cursors;

public class CursorUsuarioListado : IPageCursor<CursorUsuarioListado, UsuarioListadoDTO>
{
    public required string? LastEmail { get; set; }
    public string? Filtro { get; set; }

    public static bool HasSortColumn2 => false;

    public (string Name, object? Value) SortColumn1 => ("Email", LastEmail);

    public (string Name, object? Value)? SortColumn2 => null;

    [SetsRequiredMembers]
    public CursorUsuarioListado(string? lastEmail, string? filtro)
    {
        LastEmail = lastEmail;
        Filtro = filtro;
    }
    private CursorUsuarioListado() { }

    public static CursorUsuarioListado FromFilter(dynamic filter)
    {
        return new CursorUsuarioListado(null, filter.Filtro);
    }

    public CursorUsuarioListado Next(UsuarioListadoDTO last)
    {
        return new CursorUsuarioListado(last.Email, this.Filtro);
    }
}