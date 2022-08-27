using Pulsar.Services.Identity.Domain.Events.Grupos;
using Pulsar.Services.Identity.Domain.Specifications;
using Pulsar.Services.Shared.Enumerations;
using System.Runtime.CompilerServices;

namespace Pulsar.Services.Identity.API.Application.DomainEvents.Usuarios;

public class AjustarUsuariosEmGruposDomainEventHandler : IdentityDomainEventHandler<GrupoModificadoDomainEvent>
{
    public AjustarUsuariosEmGruposDomainEventHandler(IdentityDomainEventHandlerContext<GrupoModificadoDomainEvent> ctx) : base(ctx)
    {
    }

    protected override async Task HandleAsync(GrupoModificadoDomainEvent evt, CancellationToken ct)
    {
        var grupo = await GrupoRepository.FindOneByIdAsync(evt.GrupoId);
        if (grupo == null)
            throw new IdentityDomainException(ExceptionKey.GrupoNaoEncontrado);
        if (evt.Modificacao == ChangeEvent.Deleted)
        {
            var removerSpec = new RemoverUsuariosEmGrupoRemovidoSpec(evt.GrupoId, null, evt.UsuarioLogadoId);
            var modified = await UsuarioRepository.UpdateManyAsync(removerSpec, ct);
            grupo.AtualizarNumUsuarios(evt.UsuarioLogadoId, (int)-modified);
        }
        else if (evt.Modificacao == ChangeEvent.Edited)
        {
            if (evt.SubGruposRemovidos.Count != 0)
            {
                var removerSpec = new RemoverUsuariosEmGrupoRemovidoSpec(evt.GrupoId, evt.SubGruposRemovidos.Select(sg => sg.SubGrupoId).ToList(), evt.UsuarioLogadoId);
                var modified = await UsuarioRepository.UpdateManyAsync(removerSpec, ct);
                grupo.AtualizarNumUsuarios(evt.UsuarioLogadoId, (int)-modified);
            }
        }
    }
}
