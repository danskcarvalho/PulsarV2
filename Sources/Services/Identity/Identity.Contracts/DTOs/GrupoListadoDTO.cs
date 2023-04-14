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
