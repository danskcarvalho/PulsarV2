using System.Diagnostics.CodeAnalysis;

namespace Pulsar.Services.Identity.Contracts.DTOs;

public class GrupoListadoDTO
{
    public required string GrupoId { get; set; }
    public required string Nome { get; set; }
    public int NumSubGrupos { get; set; }
    public int NumUsuarios { get; set; }

    [SetsRequiredMembers]
    public GrupoListadoDTO(string grupoId, string nome, int numSubGrupos, int numUsuarios)
    {
        GrupoId = grupoId;
        Nome = nome;
        NumSubGrupos = numSubGrupos;
        NumUsuarios = numUsuarios;
    }
    private GrupoListadoDTO() { }
}

public class CursorGrupoListadoDTO
{
    public required string LastNome { get; set; }
    public required string LastGrupoId { get; set; }
    public string? Filtro { get; set; }

    [SetsRequiredMembers]
    public CursorGrupoListadoDTO(string lastNome, string lastGrupoId, string? filtro)
    {
        LastNome = lastNome;
        LastGrupoId = lastGrupoId;
        Filtro = filtro;
    }
    private CursorGrupoListadoDTO() { }

    public CursorGrupoListadoDTO? Next(List<GrupoListadoDTO> grupos)
    {
        if (grupos == null || grupos.Count == 0)
            return null;

        return new CursorGrupoListadoDTO(grupos.Last().Nome, grupos.Last().GrupoId, this.Filtro);
    }

    public static CursorGrupoListadoDTO? Next(List<GrupoListadoDTO> grupos, string? filtro)
    {
        if (grupos == null || grupos.Count == 0)
            return null;

        return new CursorGrupoListadoDTO(grupos.Last().Nome, grupos.Last().GrupoId, filtro);
    }
}
