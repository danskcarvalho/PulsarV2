using Pulsar.Services.Identity.Domain.Events.Grupos;
using Pulsar.Services.Identity.Domain.Specifications;

namespace Pulsar.Services.Identity.API.Application.DomainEvents.Grupos;

public class AtualizarNumUsuariosDomainEventHandler : IdentityDomainEventHandler<NumUsuariosEmGrupoModificadoDomainEvent>
{
    public AtualizarNumUsuariosDomainEventHandler(IdentityDomainEventHandlerContext<NumUsuariosEmGrupoModificadoDomainEvent> ctx) : base(ctx)
    {
    }

    protected override async Task HandleAsync(NumUsuariosEmGrupoModificadoDomainEvent evt, CancellationToken ct)
    {
        var atualizarNum = new AtualizarNumUsuariosEmGrupoSpec(evt.UsuarioLogadoId, evt.GrupoId, evt.DeltaNumUsuarios);
        await GrupoRepository.UpdateOneAsync(atualizarNum, ct);
    }
}
