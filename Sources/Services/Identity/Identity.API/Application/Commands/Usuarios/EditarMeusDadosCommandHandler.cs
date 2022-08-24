using Pulsar.Services.Identity.API.Application.BaseTypes;
using Pulsar.Services.Identity.Contracts.Commands.Usuarios;
using Pulsar.Services.Identity.Contracts.Utils;

namespace Pulsar.Services.Identity.API.Application.Commands.Usuarios;

public class EditarMeusDadosCommandHandler : IdentityCommandHandler<EditarMeusDadosCommand, CommandResult>
{
    public EditarMeusDadosCommandHandler(IdentityCommandHandlerContext<EditarMeusDadosCommand, CommandResult> ctx) : base(ctx)
    {
    }

    protected override async Task<CommandResult> HandleAsync(EditarMeusDadosCommand cmd, CancellationToken ct)
    {
        var usuario = await UsuarioRepository.FindOneByIdAsync(cmd.UsuarioId!.ToObjectId(), ct);
        if (usuario == null)
            throw new IdentityDomainException(ExceptionKey.UsuarioNaoEncontrado);
        usuario.EditarMeusDados(cmd.PrimeiroNome!, cmd.Sobrenome);
        await UsuarioRepository.ReplaceOneAsync(usuario, ct: ct);
        return new CommandResult(Session.ConsistencyToken);
    }
}
