using Pulsar.Services.Identity.Contracts.Commands.Dominios;
using Pulsar.Services.Identity.Contracts.Utils;

namespace Pulsar.Services.Identity.API.Application.Commands.Dominios;

public class CriarDominioCommandHandler : IdentityCommandHandler<CriarDominioCommand, CreatedCommandResult>
{
    public CriarDominioCommandHandler(IdentityCommandHandlerContext<CriarDominioCommand, CreatedCommandResult> ctx) : base(ctx)
    {
    }

    protected override async Task<CreatedCommandResult> HandleAsync(CriarDominioCommand cmd, CancellationToken ct)
    {
        var usuarioAdministrador = cmd.UsuarioAdministradorId != null ? await UsuarioRepository.FindOneByIdAsync(cmd.UsuarioAdministradorId.ToObjectId(), ct) : null;
        if (usuarioAdministrador == null && cmd.UsuarioAdministradorId != null)
            throw new IdentityDomainException(ExceptionKey.UsuarioNaoEncontrado);

        var dominio = new Dominio(ObjectId.GenerateNewId(), cmd.Nome!, usuarioAdministrador?.Id, new AuditInfo(cmd.UsuarioLogadoId!.ToObjectId()));
        dominio.Criar(cmd.UsuarioLogadoId!.ToObjectId(), usuarioAdministrador);
        await DominioRepository.InsertOneAsync(dominio, ct);
        return new CreatedCommandResult(dominio.Id.ToString(), Session.ConsistencyToken);
    }
}
