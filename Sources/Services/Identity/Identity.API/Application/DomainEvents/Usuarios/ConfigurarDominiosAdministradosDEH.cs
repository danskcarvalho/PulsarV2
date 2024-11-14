using Pulsar.Services.Identity.Domain.Events.Dominios;

namespace Pulsar.Services.Identity.API.Application.DomainEvents.Usuarios;

public class ConfigurarDominiosAdministradosDEH : IdentityDomainEventHandler<DominioModificadoDE>
{
    public ConfigurarDominiosAdministradosDEH(IdentityDomainEventHandlerContext<DominioModificadoDE> ctx) : base(ctx)
    {
    }

    protected override async Task HandleAsync(DominioModificadoDE evt, CancellationToken ct)
    {
        if (evt.UsuarioAdministradorAnteriorId != null && evt.UsuarioAdministradorAnteriorId != evt.UsuarioAdministradorId)
        {
            var anterior = await UsuarioRepository.FindOneByIdAsync(evt.UsuarioAdministradorAnteriorId.Value);
            if (anterior == null)
                throw new IdentityDomainException(ExceptionKey.UsuarioNaoEncontrado);
            anterior.RemoverDominioAdministrado(evt.UsuarioLogadoId, evt.DominioId);
			await UsuarioRepository.ReplaceOneAsync(anterior);
		}
        if (evt.UsuarioAdministradorId != null && evt.UsuarioAdministradorAnteriorId != evt.UsuarioAdministradorId)
        {
            var atual = await UsuarioRepository.FindOneByIdAsync(evt.UsuarioAdministradorId.Value);
            if (atual == null)
                throw new IdentityDomainException(ExceptionKey.UsuarioNaoEncontrado);
            atual.AdicionarDominioAdministrado(evt.UsuarioLogadoId, evt.DominioId);
			await UsuarioRepository.ReplaceOneAsync(atual);
		}
    }
}
