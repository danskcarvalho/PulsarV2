using Pulsar.BuildingBlocks.DDD.Attributes;
using Pulsar.Services.Identity.API.Application.BaseTypes;
using Pulsar.Services.Identity.Contracts.Commands.Convites;
using Pulsar.Services.Identity.Contracts.IntegrationEvents;

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
        {
            throw new IdentityDomainException(IdentityExceptionKey.ConviteNaoEncontrado);
        }
        var usuarioConvidante = await UsuarioRepository.FindOneByIdAsync(convite.AuditInfo.CriadoPorUsuarioId!.Value);
        if (usuarioConvidante == null)
        {
            throw new IdentityDomainException(IdentityExceptionKey.UsuarioNaoEncontrado);
		}

        convite.Aceitar(cmd.PrimeiroNome, cmd.Sobrenome, cmd.NomeUsuario, cmd.Senha, cmd.Token);
        await ConviteRepository.ReplaceOneAsync(convite);
        await EventLog.SaveEventAsync(new ConviteAceitoIE()
        {
            UsuarioConvidanteEmail = usuarioConvidante.Email ?? usuarioConvidante.PrimeiroNome ?? "Desconhecido",
            UsuarioConvidanteId = usuarioConvidante.Id.ToString(),
            UsuarioEmail = convite.Email,
            UsuarioId = convite.UsuarioId.ToString()
        });
    }
}
