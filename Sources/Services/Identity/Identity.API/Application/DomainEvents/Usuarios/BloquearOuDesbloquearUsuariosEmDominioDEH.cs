using Pulsar.Services.Identity.Domain.Events.Dominios;
using Pulsar.Services.Identity.Domain.Specifications;

namespace Pulsar.Services.Identity.API.Application.DomainEvents.Usuarios;

public class BloquearOuDesbloquearUsuariosEmDominioDEH : IdentityDomainEventHandler<UsuariosBloqueadosEmDominioDE>
{
    public BloquearOuDesbloquearUsuariosEmDominioDEH(IdentityDomainEventHandlerContext<UsuariosBloqueadosEmDominioDE> ctx) : base(ctx)
    {
    }

    protected override async Task HandleAsync(UsuariosBloqueadosEmDominioDE evt, CancellationToken ct)
    {
        var dominio = await DominioRepository.FindOneByIdAsync(evt.DominioId, ct);
        if (dominio == null)
            throw new IdentityDomainException(ExceptionKey.DominioNaoEncontrado);
        var updateUsuarios = new BloquearOuDesbloquearUsuariosEmDominioSpec(evt.UsuarioLogadoId, evt.DominioId, evt.UsuariosIds, evt.Bloquear);
        await UsuarioRepository.UpdateManyAsync(updateUsuarios, ct);
    }
}
