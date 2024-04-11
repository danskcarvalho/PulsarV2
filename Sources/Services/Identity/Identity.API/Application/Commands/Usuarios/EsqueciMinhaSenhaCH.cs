using Pulsar.BuildingBlocks.DDD.Attributes;
using Pulsar.Services.Identity.Domain.Specifications.Usuarios;

namespace Pulsar.Services.Identity.API.Application.Commands.Usuarios;

[NoTransaction, RetryOnException(VersionConcurrency = true, Retries = 2)]
public class EsqueciMinhaSenhaCH : IdentityCommandHandler<EsqueciMinhaSenhaCmd>
{
    public EsqueciMinhaSenhaCH(IdentityCommandHandlerContext<EsqueciMinhaSenhaCmd> ctx) : base(ctx)
    {
    }

    protected override async Task HandleAsync(EsqueciMinhaSenhaCmd cmd, CancellationToken ct)
    {
        var usuario = await UsuarioRepository.FindOneAsync(new FindUsuarioByUsenameOrEmailSpec(cmd.UsernameOrEmail!));
        if (usuario is null)
            throw new IdentityDomainException(ExceptionKey.UsuarioNaoEncontrado);

        await usuario.GerarTokenMudancaSenha();
    }
}
