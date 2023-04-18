using Pulsar.Services.Identity.Contracts.Utils;

namespace Pulsar.Services.Identity.Contracts.Commands.Dominios;

public class EsconderOuMostrarDominioCmd : IRequest<CommandResult>
{
    public string? UsuarioLogadoId { get; set; }
    public string? DominioId { get; set; }
    public bool Esconder { get; set; }

    public EsconderOuMostrarDominioCmd(string? usuarioLogadoId, string? dominioId, bool esconder)
    {
        UsuarioLogadoId = usuarioLogadoId;
        DominioId = dominioId;
        Esconder = esconder;
    }
}
