namespace Pulsar.Services.Identity.Contracts.DTOs;
public class CursorUsuarioGrupoListadoDTO
{
    public string LastEmail { get; set; }
    public string GrupoId { get; set; }
    public string SubGrupoId { get; set; }
    public string? Filtro { get; set; }

    public CursorUsuarioGrupoListadoDTO(string lastEmail, string grupoId, string subGrupoId, string? filtro)
    {
        LastEmail = lastEmail;
        GrupoId = grupoId;
        SubGrupoId = subGrupoId;
        Filtro = filtro;
    }

    public CursorUsuarioGrupoListadoDTO? Next(List<UsuarioListadoDTO> usuarios)
    {
        if (usuarios == null || usuarios.Count == 0)
            return null;

        return new CursorUsuarioGrupoListadoDTO(usuarios.Last().Email, this.GrupoId, this.SubGrupoId, this.Filtro);
    }

    public static CursorUsuarioGrupoListadoDTO? Next(List<UsuarioListadoDTO> usuarios, string grupoId, string subgrupoId, string? filtro)
    {
        if (usuarios == null || usuarios.Count == 0)
            return null;

        return new CursorUsuarioGrupoListadoDTO(usuarios.Last().Email, grupoId, subgrupoId, filtro);
    }
}