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
            throw new IdentityDomainException(IdentityExceptionKey.UsuarioNaoEncontrado);
        usuario.EditarMeusDados(cmd.PrimeiroNome!, cmd.Sobrenome);
		await UsuarioRepository.ReplaceOneAsync(usuario);
		return new CommandResult(Session.ConsistencyToken);
    }
}
