﻿using Pulsar.Services.Identity.Contracts.Commands.Grupos;

namespace Pulsar.Services.Identity.API.Application.Commands.Grupos;

public class RemoverSubGrupoCH : IdentityCommandHandler<RemoverSubGrupoCmd, CommandResult>
{
    public RemoverSubGrupoCH(IdentityCommandHandlerContext<RemoverSubGrupoCmd, CommandResult> ctx) : base(ctx)
    {
    }

    protected override async Task<CommandResult> HandleAsync(RemoverSubGrupoCmd cmd, CancellationToken ct)
    {
        var grupo = await GrupoRepository.FindOneByIdAsync(cmd.GrupoId!.ToObjectId());
        if (grupo == null || grupo.DominioId != cmd.DominioId!.ToObjectId())
            throw new IdentityDomainException(IdentityExceptionKey.GrupoNaoEncontrado);
        grupo.RemoverSubGrupo(cmd.UsuarioLogadoId!.ToObjectId(), cmd.SubGrupoId!.ToObjectId());
		await GrupoRepository.ReplaceOneAsync(grupo);
		return new CommandResult(Session.ConsistencyToken);
    }
}
