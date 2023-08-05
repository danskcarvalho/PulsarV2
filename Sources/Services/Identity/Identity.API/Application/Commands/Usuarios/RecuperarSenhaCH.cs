using Pulsar.BuildingBlocks.DDD.Attributes;
using Pulsar.Services.Identity.API.Application.BaseTypes;
using Pulsar.Services.Identity.Contracts.Commands.Usuarios;

namespace Pulsar.Services.Identity.API.Application.Commands.Usuarios;

[NoTransaction, RetryOnException(VersionConcurrency = true, Retries = 2)]
public class RecuperarSenhaCH : IdentityCommandHandler<RecuperarSenhaCmd>
{
    public RecuperarSenhaCH(IdentityCommandHandlerContext<RecuperarSenhaCmd> ctx) : base(ctx)
    {
    }

    protected override async Task HandleAsync(RecuperarSenhaCmd cmd, CancellationToken ct)
    {
        var usuario = await UsuarioRepository.FindOneByIdAsync(cmd.UsuarioId!.ToObjectId());
        if (usuario == null)
            throw new IdentityDomainException(ExceptionKey.UsuarioNaoEncontrado);

        await usuario.RecuperarSenha(cmd.Token!, cmd.Senha!);
    }
}
