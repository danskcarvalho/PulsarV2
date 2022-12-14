using Pulsar.Services.Identity.Contracts.Commands.Dominios;
using Pulsar.Services.Identity.Contracts.Utils;
using Pulsar.Services.Identity.Domain.Specifications;

namespace Pulsar.Services.Identity.API.Application.Commands.Dominios;

public class EditarDominioCommandHandler : IdentityCommandHandler<EditarDominioCommand, CommandResult>
{
    public EditarDominioCommandHandler(IdentityCommandHandlerContext<EditarDominioCommand, CommandResult> ctx) : base(ctx)
    {
    }

    protected override async Task<CommandResult> HandleAsync(EditarDominioCommand cmd, CancellationToken ct)
    {
        var usuarioAdministrador = cmd.UsuarioAdministradorId != null ? await UsuarioRepository.FindOneByIdAsync(cmd.UsuarioAdministradorId.ToObjectId(), ct) : null;
        if (usuarioAdministrador == null && cmd.UsuarioAdministradorId != null)
            throw new IdentityDomainException(ExceptionKey.UsuarioNaoEncontrado);
        var dominio = await DominioRepository.FindOneByIdAsync(cmd.DominioId!.ToObjectId(), ct);
        if (dominio == null)
            throw new IdentityDomainException(ExceptionKey.DominioNaoEncontrado);
        var usuariosBloqueados = usuarioAdministrador != null ? await UsuarioRepository.FindManyAsync(new FindUsuariosBloqueadosDominioSpec(dominio.Id, usuarioAdministrador.Id)) : null;
        dominio.Editar(cmd.UsuarioLogadoId!.ToObjectId(), cmd.Nome!, usuarioAdministrador, usuariosBloqueados);
        await DominioRepository.ReplaceOneAsync(dominio, ct: ct);
        return new CommandResult(Session.ConsistencyToken);
    }
}
