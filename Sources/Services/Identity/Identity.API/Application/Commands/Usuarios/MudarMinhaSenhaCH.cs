using Pulsar.BuildingBlocks.DDD.Attributes;

namespace Pulsar.Services.Identity.API.Application.Commands.Usuarios;

[NoTransaction, RetryOnException(VersionConcurrency = true, Retries = 2)]
public class MudarMinhaSenhaCH : IdentityCommandHandler<MudarMinhaSenhaCmd, CommandResult>
{
    public MudarMinhaSenhaCH(IdentityCommandHandlerContext<MudarMinhaSenhaCmd, CommandResult> ctx) : base(ctx)
    {
    }

    protected override async Task<CommandResult> HandleAsync(MudarMinhaSenhaCmd cmd, CancellationToken ct)
    {
        var usuario = await UsuarioRepository.FindOneByIdAsync(cmd.UsuarioId!.ToObjectId());
        if (usuario == null)
            throw new IdentityDomainException(IdentityExceptionKey.UsuarioNaoEncontrado);
        usuario.MudarMinhaSenha(cmd.SenhaAtual!, cmd.Senha!);
		await UsuarioRepository.ReplaceOneAsync(usuario).CheckModified();
		return new CommandResult(Session.ConsistencyToken);
    }
}
