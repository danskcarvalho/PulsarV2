namespace Pulsar.Services.Identity.Contracts.Commands.Grupos;

public class RemoverGrupoCmd : IRequest<CommandResult>
{
    public string? UsuarioLogadoId { get; set; }
    public string? DominioId { get; set; }
    public string? GrupoId { get; set; }
}
