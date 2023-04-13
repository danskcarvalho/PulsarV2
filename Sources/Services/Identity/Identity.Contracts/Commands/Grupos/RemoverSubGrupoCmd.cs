using Pulsar.Services.Identity.Contracts.Utils;
using Pulsar.Services.Shared.Attributes;

namespace Pulsar.Services.Identity.Contracts.Commands.Grupos;

public class RemoverSubGrupoCmd : IRequest<CommandResult>
{
    public string? UsuarioLogadoId { get; set; }
    public string? DominioId { get; set; }
    public string? GrupoId { get; set; }
    public string? SubGrupoId { get; set; }
}
