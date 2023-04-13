using Pulsar.BuildingBlocks.DDD.Abstractions;
using Pulsar.BuildingBlocks.DDD.Attributes;
using Pulsar.Services.Identity.API.Application.BaseTypes;
using Pulsar.Services.Identity.Contracts.Commands.Usuarios;
using Pulsar.Services.Identity.Domain.Specifications;

namespace Pulsar.Services.Identity.API.Application.Commands.Usuarios;

[NoTransaction, RetryOnException(VersionConcurrency = true, Retries = 2)]
public class EsqueciMinhaSenhaCH : IdentityCommandHandler<EsqueciMinhaSenhaCmd>
{
    public EsqueciMinhaSenhaCH(IdentityCommandHandlerContext<EsqueciMinhaSenhaCmd> ctx) : base(ctx)
    {
    }

    protected override async Task HandleAsync(EsqueciMinhaSenhaCmd cmd, CancellationToken ct)
    {
        var usuario = await UsuarioRepository.FindOneAsync(new FindUsuarioByUsenameOrEmailSpec(cmd.UsernameOrEmail!), ct);
        if (usuario is null)
            throw new IdentityDomainException(ExceptionKey.UsuarioNaoEncontrado);

        usuario.GerarTokenMudancaSenha(out long previousVersion);
        await UsuarioRepository.ReplaceOneAsync(usuario, previousVersion, ct).CheckModified();
    }
}
