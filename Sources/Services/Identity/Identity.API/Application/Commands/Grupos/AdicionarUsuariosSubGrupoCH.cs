using Pulsar.Services.Identity.Contracts.Commands.Grupos;

namespace Pulsar.Services.Identity.API.Application.Commands.Grupos;

public class AdicionarUsuariosSubGrupoCH : IdentityCommandHandler<AdicionarUsuariosSubGrupoCmd, CommandResult>
{
    public AdicionarUsuariosSubGrupoCH(IdentityCommandHandlerContext<AdicionarUsuariosSubGrupoCmd, CommandResult> ctx) : base(ctx)
    {
    }

    protected override async Task<CommandResult> HandleAsync(AdicionarUsuariosSubGrupoCmd cmd, CancellationToken ct)
    {
        var grupo = await GrupoRepository.FindOneByIdAsync(cmd.GrupoId!.ToObjectId());
        if (grupo == null || grupo.DominioId != cmd.DominioId!.ToObjectId())
            throw new IdentityDomainException(ExceptionKey.GrupoNaoEncontrado);
        if (!await UsuarioRepository.AllExistsAsync(cmd.UsuarioIds!.Select(u => u.ToObjectId())))
            throw new IdentityDomainException(ExceptionKey.UsuarioNaoEncontrado);

        grupo.AdicionarUsuariosEmSubGrupo(cmd.UsuarioLogadoId!.ToObjectId(), cmd.SubGrupoId!.ToObjectId(), cmd.UsuarioIds!.Select(x => x.ToObjectId()).ToList());
        await GrupoRepository.ReplaceOneAsync(grupo);
        return new CommandResult(Session.ConsistencyToken);
    }
}
