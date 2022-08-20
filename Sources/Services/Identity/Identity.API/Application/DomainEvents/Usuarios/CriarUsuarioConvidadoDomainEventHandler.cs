using Pulsar.Services.Identity.API.Application.BaseTypes;
using Pulsar.Services.Identity.Domain.Aggregates.Convites;
using Pulsar.Services.Identity.Domain.Events.Convites;

namespace Pulsar.Services.Identity.API.Application.DomainEvents.Usuarios;

public class CriarUsuarioConvidadoDomainEventHandler : IdentityDomainEventHandler<UsuarioConvidadoDomainEvent>
{
    public CriarUsuarioConvidadoDomainEventHandler(ILogger<IdentityDomainEventHandler<UsuarioConvidadoDomainEvent>> logger, IDbSession session, IEnumerable<IIsRepository> repositories) : base(logger, session, repositories)
    {
    }

    protected override async Task HandleAsync(UsuarioConvidadoDomainEvent evt, CancellationToken ct)
    {
        var usuario = new Usuario(
            evt.UsuarioId,
            evt.Email!,
            evt.Email,
            Guid.NewGuid().ToString("N"),
            GeneralExtensions.GetSalt(),
            GeneralExtensions.GetSalt(),
            new AuditInfo(evt.UsuarioLogadoId))
        {
            IsAtivo = false,
            IsConvitePendente = true
        };

        await UsuarioRepository.InsertOneAsync(usuario);
    }
}
