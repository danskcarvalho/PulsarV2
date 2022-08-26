namespace Pulsar.Services.Identity.Contracts.DTOs;

public class GrupoListadoDTO
{
    public string GrupoId { get; set; }
    public string Nome { get; set; }
    public int NumSubGrupos { get; set; }
    public int NumUsuarios { get; set; }

    public GrupoListadoDTO(string grupoId, string nome, int numSubGrupos, int numUsuarios)
    {
        GrupoId = grupoId;
        Nome = nome;
        NumSubGrupos = numSubGrupos;
        NumUsuarios = numUsuarios;
    }
}

public class CursorGrupoListadoDTO
{
    public string LastNome { get; set; }
    public string LastGrupoId { get; set; }
    public string? Filtro { get; set; }

    public CursorGrupoListadoDTO(string lastNome, string lastGrupoId, string? filtro)
    {
        LastNome = lastNome;
        LastGrupoId = lastGrupoId;
        Filtro = filtro;
    }

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
