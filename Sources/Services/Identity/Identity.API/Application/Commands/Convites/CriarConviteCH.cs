using Pulsar.BuildingBlocks.DDD.Attributes;
using Pulsar.Services.Identity.API.Application.BaseTypes;
using Pulsar.Services.Identity.Contracts.Commands.Convites;
using Pulsar.Services.Identity.Domain.Specifications.Usuarios;

namespace Pulsar.Services.Identity.API.Application.Commands.Convites;

[RetryOnException(DuplicatedKey = true)]
public class CriarConviteCH : IdentityCommandHandler<CriarConviteCmd>
{
    public CriarConviteCH(IdentityCommandHandlerContext<CriarConviteCmd> ctx) : base(ctx)
    {
    }

    protected override async Task HandleAsync(CriarConviteCmd cmd, CancellationToken ct)
    {
        cmd.Email = cmd.Email?.ToLowerInvariant().Trim();
        var usuarioExistente = await UsuarioRepository.FindOneAsync(new FindUsuarioByEitherUsenameOrEmailSpec(null, cmd.Email));
        if (usuarioExistente != null && !usuarioExistente.IsConvitePendente)
            throw new IdentityDomainException(ExceptionKey.UsuarioJaConvidado);

        var convite = new Convite(ObjectId.GenerateNewId(), cmd.Email!, DateTime.UtcNow.AddDays(1), GeneralExtensions.GetSalt(), usuarioExistente?.Id ?? ObjectId.GenerateNewId(), new AuditInfo(cmd.UsuarioLogadoId!.ToObjectId()));
        convite.ConvidarUsuario();
        await ConviteRepository.InsertOneAsync(convite);
    }
}
