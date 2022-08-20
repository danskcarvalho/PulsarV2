using Pulsar.BuildingBlocks.DDD.Attributes;
using Pulsar.Services.Identity.API.Application.BaseTypes;
using Pulsar.Services.Identity.Contracts.Commands.Convites;
using Pulsar.Services.Identity.Domain.Specifications;

namespace Pulsar.Services.Identity.API.Application.Commands.Convites;

[RetryOnException(DuplicatedKey = true)]
public class CriarConviteCommandHandler : IdentityCommandHandler<CriarConviteCommand>
{
    public CriarConviteCommandHandler(ILogger<IdentityCommandHandler<CriarConviteCommand>> logger, IDbSession session, IEnumerable<IIsRepository> repositories) : base(logger, session, repositories)
    {
    }

    protected override async Task HandleAsync(CriarConviteCommand cmd, CancellationToken ct)
    {
        cmd.Email = cmd.Email?.ToLowerInvariant().Trim();
        var usuarioExistente = await UsuarioRepository.FindOneAsync(new FindUsuarioByEitherUsenameOrEmailSpec(null, cmd.Email), ct);
        if (usuarioExistente != null && !usuarioExistente.IsConvitePendente)
            throw new IdentityDomainException(ExceptionKey.UsuarioJaConvidado);

        var convite = new Convite(ObjectId.GenerateNewId(), cmd.Email!, DateTime.UtcNow.AddDays(1), GeneralExtensions.GetSalt(), ObjectId.GenerateNewId(), new AuditInfo(cmd.UsuarioLogadoId!.ToObjectId()));
        convite.ConvidarUsuario();

        await ConviteRepository.InsertOneAsync(convite, ct);
    }
}
