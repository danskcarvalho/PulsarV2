using Pulsar.Services.Identity.API.Application.BaseTypes;
using Pulsar.Services.Identity.Domain.Events.Convites;

namespace Pulsar.Services.Identity.API.Application.DomainEvents.Dominios;

public class SetAdministradorUsuarioConviadoDomainEventHandler : IdentityDomainEventHandler<ConviteAceitoDomainEvent>
{
    public SetAdministradorUsuarioConviadoDomainEventHandler(ILogger<IdentityDomainEventHandler<ConviteAceitoDomainEvent>> logger, IDbSession session, IEnumerable<IIsRepository> repositories) : base(logger, session, repositories)
    {
    }

    protected override async Task HandleAsync(ConviteAceitoDomainEvent evt, CancellationToken ct)
    {
        if (evt.AdministrarDominio)
        {
            var dominio = await DominioRepository.FindOneByIdAsync(evt.DominioId, ct);
            if (dominio == null)
                return;
            dominio.SetAdministradorDominio(evt.UsuarioId, evt.CriadoPorUsuarioId);
            await DominioRepository.ReplaceOneAsync(dominio, ct: ct);
        }
    }
}
