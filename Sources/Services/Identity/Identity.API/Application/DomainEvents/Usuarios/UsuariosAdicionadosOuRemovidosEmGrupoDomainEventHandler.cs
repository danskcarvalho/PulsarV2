using Pulsar.Services.Identity.Domain.Events.Grupos;
using Pulsar.Services.Identity.Domain.Specifications;

namespace Pulsar.Services.Identity.API.Application.DomainEvents.Usuarios;

public class UsuariosAdicionadosOuRemovidosEmGrupoDomainEventHandler : IdentityDomainEventHandler<UsuariosAdicionadosOuRemovidosEmGrupoDomainEvent>
{
    public UsuariosAdicionadosOuRemovidosEmGrupoDomainEventHandler(IdentityDomainEventHandlerContext<UsuariosAdicionadosOuRemovidosEmGrupoDomainEvent> ctx) : base(ctx)
    {
    }

    protected override async Task HandleAsync(UsuariosAdicionadosOuRemovidosEmGrupoDomainEvent evt, CancellationToken ct)
    {
        var grupo = await GrupoRepository.FindOneByIdAsync(evt.GrupoId);
        if (grupo == null)
            throw new IdentityDomainException(ExceptionKey.GrupoNaoEncontrado);
        if (evt.Remocao)
        {
            var spec = new RemoverUsuariosEmGrupoSpec(evt.UsuarioLogadoId, evt.DominioId, evt.GrupoId, evt.SubGrupoId, evt.UsuarioIds);
            await UsuarioRepository.UpdateManyAsync(spec, ct);
            grupo.AtualizarNumUsuarios(evt.UsuarioLogadoId, new List<ObjectId> { evt.SubGrupoId });
        }
        else
        {
            var spec = new AdicionarUsuariosEmGruposSpec(evt.UsuarioLogadoId, evt.DominioId, evt.GrupoId, evt.SubGrupoId, evt.UsuarioIds);
            await UsuarioRepository.UpdateManyAsync(spec, ct);
            grupo.AtualizarNumUsuarios(evt.UsuarioLogadoId, new List<ObjectId> { evt.SubGrupoId });
        }
    }
}
