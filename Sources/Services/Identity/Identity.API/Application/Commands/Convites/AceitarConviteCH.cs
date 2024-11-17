using Pulsar.BuildingBlocks.DDD.Attributes;
using Pulsar.Services.Identity.API.Application.BaseTypes;
using Pulsar.Services.Identity.Contracts.Commands.Convites;

namespace Pulsar.Services.Identity.API.Application.Commands.Convites;

[RetryOnException(DuplicatedKey = true)]
public class AceitarConviteCH : IdentityCommandHandler<AceitarConviteCmd>
{
    public AceitarConviteCH(IdentityCommandHandlerContext<AceitarConviteCmd> ctx) : base(ctx)
    {
    }

    protected override async Task HandleAsync(AceitarConviteCmd cmd, CancellationToken ct)
    {
        var convite = await ConviteRepository.FindOneByIdAsync(cmd.ConviteId!.ToObjectId());
        if (convite == null)
            throw new IdentityDomainException(IdentityExceptionKey.ConviteNaoEncontrado);

        convite.Aceitar(cmd.PrimeiroNome, cmd.Sobrenome, cmd.NomeUsuario, cmd.Senha, cmd.Token);
        await ConviteRepository.ReplaceOneAsync(convite);
    }
}
