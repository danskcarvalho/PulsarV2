using Pulsar.Services.Identity.Domain.Events.Dominios;
using Pulsar.Services.Identity.Domain.Specifications;

namespace Pulsar.Services.Identity.API.Application.DomainEvents.Usuarios;

public class BloquearOuDesbloquearUsuariosEmDominioDomainEventHandler : IdentityDomainEventHandler<UsuariosBloqueadosEmDominioDomainEvent>
{
    public BloquearOuDesbloquearUsuariosEmDominioDomainEventHandler(IdentityDomainEventHandlerContext<UsuariosBloqueadosEmDominioDomainEvent> ctx) : base(ctx)
    {
    }

    protected override async Task HandleAsync(UsuariosBloqueadosEmDominioDomainEvent evt, CancellationToken ct)
    {
        var dominio = await DominioRepository.FindOneByIdAsync(evt.DominioId, ct);
        if (dominio == null)
            throw new IdentityDomainException(ExceptionKey.DominioNaoEncontrado);
        var updateUsuarios = new BloquearOuDesbloquearUsuariosEmDominioSpec(evt.UsuarioLogadoId, evt.DominioId, evt.UsuariosIds, evt.Bloquear);
        dominio.UsuariosBloqueados(evt.UsuarioLogadoId, evt.UsuariosIds, evt.Bloquear);
    }
}
