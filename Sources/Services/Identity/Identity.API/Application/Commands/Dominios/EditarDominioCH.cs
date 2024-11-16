using Pulsar.Services.Identity.Contracts.Commands.Dominios;
using Pulsar.Services.Identity.Domain.Specifications.Usuarios;

namespace Pulsar.Services.Identity.API.Application.Commands.Dominios;

public class EditarDominioCH : IdentityCommandHandler<EditarDominioCmd, CommandResult>
{
    public EditarDominioCH(IdentityCommandHandlerContext<EditarDominioCmd, CommandResult> ctx) : base(ctx)
    {
    }

    protected override async Task<CommandResult> HandleAsync(EditarDominioCmd cmd, CancellationToken ct)
    {
        var usuarioAdministrador = cmd.UsuarioAdministradorId != null ? await UsuarioRepository.FindOneByIdAsync(cmd.UsuarioAdministradorId.ToObjectId(), ct) : null;
        if (usuarioAdministrador == null && cmd.UsuarioAdministradorId != null)
            throw new IdentityDomainException(ExceptionKey.UsuarioNaoEncontrado);
        var dominio = await DominioRepository.FindOneByIdAsync(cmd.DominioId!.ToObjectId());
        if (dominio == null)
            throw new IdentityDomainException(ExceptionKey.DominioNaoEncontrado);
        var usuariosBloqueados = usuarioAdministrador != null ? await UsuarioRepository.FindManyAsync(new FindUsuariosBloqueadosDominioSpec(dominio.Id, usuarioAdministrador.Id)) : null;
        dominio.Editar(cmd.UsuarioLogadoId!.ToObjectId(), cmd.Nome!, usuarioAdministrador, usuariosBloqueados);
        await DominioRepository.ReplaceOneAsync(dominio);
        return new CommandResult(Session.ConsistencyToken);
    }
}
