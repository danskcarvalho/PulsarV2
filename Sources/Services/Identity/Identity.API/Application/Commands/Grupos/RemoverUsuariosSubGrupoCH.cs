﻿using Pulsar.Services.Identity.Contracts.Commands.Grupos;

namespace Pulsar.Services.Identity.API.Application.Commands.Grupos;

public class RemoverUsuariosSubGrupoCH : IdentityCommandHandler<RemoverUsuariosSubGrupoCmd, CommandResult>
{
    public RemoverUsuariosSubGrupoCH(IdentityCommandHandlerContext<RemoverUsuariosSubGrupoCmd, CommandResult> ctx) : base(ctx)
    {
    }

    protected override async Task<CommandResult> HandleAsync(RemoverUsuariosSubGrupoCmd cmd, CancellationToken ct)
    {
        var grupo = await GrupoRepository.FindOneByIdAsync(cmd.GrupoId!.ToObjectId());
        if (grupo == null || grupo.DominioId != cmd.DominioId!.ToObjectId())
            throw new IdentityDomainException(IdentityExceptionKey.GrupoNaoEncontrado);
        if (!await UsuarioRepository.AllExistsAsync(cmd.UsuarioIds!.Select(u => u.ToObjectId())))
            throw new IdentityDomainException(IdentityExceptionKey.UsuarioNaoEncontrado);

        grupo.RemoverUsuariosEmSubGrupo(cmd.UsuarioLogadoId!.ToObjectId(), cmd.SubGrupoId!.ToObjectId(), cmd.UsuarioIds!.Select(x => x.ToObjectId()).ToList());
		await GrupoRepository.ReplaceOneAsync(grupo);
		return new CommandResult(Session.ConsistencyToken);
    }
}
