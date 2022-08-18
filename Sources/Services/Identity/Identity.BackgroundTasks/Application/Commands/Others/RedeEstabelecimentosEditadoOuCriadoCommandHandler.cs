namespace Pulsar.Services.Identity.BackgroundTasks.Application.Commands.Others;

[NoTransaction, RetryOnException(DuplicatedKey = true, VersionConcurrency = true, Retries = 2)]
public class RedeEstabelecimentosEditadoOuCriadoCommandHandler : IdentityCommandHandler<RedeEstabelecimentosEditadoOuCriadoCommand>
{
    public RedeEstabelecimentosEditadoOuCriadoCommandHandler(ILogger<IdentityCommandHandler<RedeEstabelecimentosEditadoOuCriadoCommand>> logger, IDbSession session, IEnumerable<IIsRepository> repositories) : base(logger, session, repositories)
    {
    }

    protected override async Task HandleAsync(RedeEstabelecimentosEditadoOuCriadoCommand cmd, CancellationToken ct)
    {
        var rede = await RedeEstabelecimentosRepository.FindOneByIdAsync(cmd.RedeEstabelecimentsoId.ToObjectId(), ct);
        if (rede == null)
        {
            rede = new RedeEstabelecimentos(
                cmd.RedeEstabelecimentsoId.ToObjectId(),
                cmd.DominioId.ToObjectId(),
                cmd.Nome,
                cmd.TimeStamp);
            await RedeEstabelecimentosRepository.InsertOneAsync(rede);
        }
        else
        {
            bool editado = rede.Editar(cmd.Nome, cmd.TimeStamp, out long previousVersion);
            if (editado)
                await RedeEstabelecimentosRepository.ReplaceOneAsync(rede, previousVersion, ct).CheckModified();
        }
    }
}
