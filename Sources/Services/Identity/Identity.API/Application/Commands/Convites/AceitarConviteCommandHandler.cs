using Pulsar.BuildingBlocks.DDD.Attributes;
using Pulsar.Services.Identity.API.Application.BaseTypes;
using Pulsar.Services.Identity.Contracts.Commands.Convites;

namespace Pulsar.Services.Identity.API.Application.Commands.Convites;

[RetryOnException(DuplicatedKey = true)]
public class AceitarConviteCommandHandler : IdentityCommandHandler<AceitarConviteCommand>
{
    public AceitarConviteCommandHandler(IdentityCommandHandlerContext<AceitarConviteCommand> ctx) : base(ctx)
    {
    }

    protected override async Task HandleAsync(AceitarConviteCommand cmd, CancellationToken ct)
    {
        var convite = await ConviteRepository.FindOneByIdAsync(cmd.ConviteId!.ToObjectId(), ct);
        if (convite == null)
            throw new IdentityDomainException(ExceptionKey.ConviteNaoEncontrado);

        convite.Aceitar(cmd.PrimeiroNome, cmd.Sobrenome, cmd.NomeUsuario, cmd.Senha, cmd.Token);
        await ConviteRepository.ReplaceOneAsync(convite, ct: ct);
    }
}
