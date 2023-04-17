using Pulsar.Services.Identity.Contracts.Commands.Dominios;
using Pulsar.Services.Identity.Contracts.Utils;

namespace Pulsar.Services.Identity.API.Application.Commands.Dominios;

public class CriarDominioCH : IdentityCommandHandler<CriarDominioCmd, CreatedCommandResult>
{
    public CriarDominioCH(IdentityCommandHandlerContext<CriarDominioCmd, CreatedCommandResult> ctx) : base(ctx)
    {
    }

    protected override async Task<CreatedCommandResult> HandleAsync(CriarDominioCmd cmd, CancellationToken ct)
    {
        var usuarioAdministrador = cmd.UsuarioAdministradorId != null ? await UsuarioRepository.FindOneByIdAsync(cmd.UsuarioAdministradorId.ToObjectId()) : null;
        if (usuarioAdministrador == null && cmd.UsuarioAdministradorId != null)
            throw new IdentityDomainException(ExceptionKey.UsuarioNaoEncontrado);

        var dominio = new Dominio(ObjectId.GenerateNewId(), cmd.Nome!, usuarioAdministrador?.Id, new AuditInfo(cmd.UsuarioLogadoId!.ToObjectId()));
        dominio.Criar(cmd.UsuarioLogadoId!.ToObjectId(), usuarioAdministrador);
        await DominioRepository.InsertOneAsync(dominio);
        return new CreatedCommandResult(dominio.Id.ToString(), Session.ConsistencyToken);
    }
}
