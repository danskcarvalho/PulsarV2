using Pulsar.Services.Identity.Contracts.Commands.Dominios;
using Pulsar.Services.Identity.Contracts.Utils;
using Pulsar.Services.Identity.Domain.Specifications;

namespace Pulsar.Services.Identity.API.Application.Commands.Dominios;

public class EsconderOuMostrarDominioCH : IdentityCommandHandler<EsconderOuMostrarDominioCmd, CommandResult>
{
    public EsconderOuMostrarDominioCH(IdentityCommandHandlerContext<EsconderOuMostrarDominioCmd, CommandResult> ctx) : base(ctx)
    {
    }

    protected override async Task<CommandResult> HandleAsync(EsconderOuMostrarDominioCmd cmd, CancellationToken ct)
    {
        var dominio = await DominioRepository.FindOneByIdAsync(cmd.DominioId!.ToObjectId());
        if (dominio == null)
            throw new IdentityDomainException(ExceptionKey.DominioNaoEncontrado);
        if (cmd.Esconder)
            await dominio.Esconder(cmd.UsuarioLogadoId!.ToObjectId());
        else
            await dominio.Mostrar(cmd.UsuarioLogadoId!.ToObjectId());
        return new CommandResult(Session.ConsistencyToken);
    }
}
