using Pulsar.Services.Identity.Domain.Events.Grupos;
using Pulsar.Services.Identity.Domain.Specifications;

namespace Pulsar.Services.Identity.API.Application.DomainEvents.Grupos;

public class AtualizarNumUsuariosDEH : IdentityDomainEventHandler<NumUsuariosEmGrupoModificadoDE>
{
    public AtualizarNumUsuariosDEH(IdentityDomainEventHandlerContext<NumUsuariosEmGrupoModificadoDE> ctx) : base(ctx)
    {
    }

    protected override async Task HandleAsync(NumUsuariosEmGrupoModificadoDE evt, CancellationToken ct)
    {
        await GrupoRepository.AtualizarNumUsuarios(evt.UsuarioLogadoId, evt.GrupoId, evt.SubGrupoIds);
    }
}
