using Pulsar.Services.Identity.API.Application.BaseTypes;
using Pulsar.Services.Identity.Contracts.Commands.Usuarios;
using Pulsar.Services.Identity.Contracts.Utils;

namespace Pulsar.Services.Identity.API.Application.Commands.Usuarios;

public class EditarMeusDadosCH : IdentityCommandHandler<EditarMeusDadosCmd, CommandResult>
{
    public EditarMeusDadosCH(IdentityCommandHandlerContext<EditarMeusDadosCmd, CommandResult> ctx) : base(ctx)
    {
    }

    protected override async Task<CommandResult> HandleAsync(EditarMeusDadosCmd cmd, CancellationToken ct)
    {
        var usuario = await UsuarioRepository.FindOneByIdAsync(cmd.UsuarioId!.ToObjectId());
        if (usuario == null)
            throw new IdentityDomainException(ExceptionKey.UsuarioNaoEncontrado);
        await usuario.EditarMeusDados(cmd.PrimeiroNome!, cmd.Sobrenome);
        return new CommandResult(Session.ConsistencyToken);
    }
}
