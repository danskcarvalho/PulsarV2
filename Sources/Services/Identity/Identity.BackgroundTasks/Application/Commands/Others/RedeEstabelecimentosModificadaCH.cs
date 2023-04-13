using Pulsar.Services.Identity.BackgroundTasks.Utils;

namespace Pulsar.Services.Identity.BackgroundTasks.Application.Commands.Others;

[NoTransaction, RetryOnException(DuplicatedKey = true, VersionConcurrency = true, Retries = 2)]
public class RedeEstabelecimentosModificadaCH : IdentityCommandHandler<RedeEstabelecimentosEditadaOuCriadaCommand>
{
    public RedeEstabelecimentosModificadaCH(IdentityCommandHandlerContext<RedeEstabelecimentosEditadaOuCriadaCommand> ctx) : base(ctx)
    {
    }

    protected override async Task HandleAsync(RedeEstabelecimentosEditadaOuCriadaCommand cmd, CancellationToken ct)
    {
        var rede = await RedeEstabelecimentosRepository.FindOneByIdAsync(cmd.RedeEstabelecimentsoId.ToObjectId(), ct);
        if (rede == null)
        {
            rede = new RedeEstabelecimentos(
                cmd.RedeEstabelecimentsoId.ToObjectId(),
                cmd.DominioId.ToObjectId(),
                cmd.Nome,
                cmd.AuditInfo.ToDomainObject(),
                cmd.TimeStamp);
            await RedeEstabelecimentosRepository.InsertOneAsync(rede);
        }
        else
        {
            bool editado = rede.Editar(cmd.Nome, cmd.AuditInfo.ToDomainObject(), cmd.TimeStamp, out long previousVersion);
            if (editado)
                await RedeEstabelecimentosRepository.ReplaceOneAsync(rede, previousVersion, ct).CheckModified();
        }
    }
}
