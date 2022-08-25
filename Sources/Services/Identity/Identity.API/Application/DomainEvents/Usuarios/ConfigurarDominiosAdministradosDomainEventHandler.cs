using Pulsar.Services.Identity.Domain.Events.Dominios;

namespace Pulsar.Services.Identity.API.Application.DomainEvents.Usuarios;

public class ConfigurarDominiosAdministradosDomainEventHandler : IdentityDomainEventHandler<DominioModificadoDomainEvent>
{
    public ConfigurarDominiosAdministradosDomainEventHandler(IdentityDomainEventHandlerContext<DominioModificadoDomainEvent> ctx) : base(ctx)
    {
    }

    protected override async Task HandleAsync(DominioModificadoDomainEvent evt, CancellationToken ct)
    {
        if (evt.UsuarioAdministradorAnteriorId != null && evt.UsuarioAdministradorAnteriorId != evt.UsuarioAdministradorId)
        {
            var anterior = await UsuarioRepository.FindOneByIdAsync(evt.UsuarioAdministradorAnteriorId.Value, ct);
            if (anterior == null)
                throw new IdentityDomainException(ExceptionKey.UsuarioNaoEncontrado);
            anterior.RemoverDominioAdministrado(evt.UsuarioLogadoId, evt.DominioId);
            await UsuarioRepository.ReplaceOneAsync(anterior, ct: ct);
        }
        if (evt.UsuarioAdministradorId != null && evt.UsuarioAdministradorAnteriorId != evt.UsuarioAdministradorId)
        {
            var atual = await UsuarioRepository.FindOneByIdAsync(evt.UsuarioAdministradorId.Value, ct);
            if (atual == null)
                throw new IdentityDomainException(ExceptionKey.UsuarioNaoEncontrado);
            atual.AdicionarDominioAdministrado(evt.UsuarioLogadoId, evt.DominioId);
            await UsuarioRepository.ReplaceOneAsync(atual, ct: ct);
        }
    }
}
