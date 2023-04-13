namespace Pulsar.Services.Identity.BackgroundTasks.Application.Commands.Others;

[NoTransaction, RetryOnException(DuplicatedKey = true, VersionConcurrency = true, Retries = 2)]
public class EstabelecimentoModificadoCH : IdentityCommandHandler<EstabelecimentoModificadoCmd>
{
    public EstabelecimentoModificadoCH(IdentityCommandHandlerContext<EstabelecimentoModificadoCmd> ctx) : base(ctx)
    {
    }

    protected override async Task HandleAsync(EstabelecimentoModificadoCmd cmd, CancellationToken ct)
    {
        var estabelecimento = await EstabelecimentoRepository.FindOneByIdAsync(cmd.EstabelecimentoId.ToObjectId(), ct);
        if (estabelecimento == null)
        {
            estabelecimento = new Estabelecimento(
                cmd.EstabelecimentoId.ToObjectId(),
                cmd.DominioId.ToObjectId(),
                cmd.Nome,
                cmd.Cnes,
                cmd.Redes.Select(r => r.ToObjectId()),
                cmd.IsAtivo,
                cmd.TimeStamp);
            await EstabelecimentoRepository.InsertOneAsync(estabelecimento);
        }
        else
        {
            bool editado = estabelecimento.Editar(cmd.Nome, cmd.Cnes, cmd.Redes.Select(r => r.ToObjectId()), cmd.IsAtivo, cmd.TimeStamp, out long previousVersion);
            if (editado)
                await EstabelecimentoRepository.ReplaceOneAsync(estabelecimento, previousVersion, ct).CheckModified();
        }
    }
}
