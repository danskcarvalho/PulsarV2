namespace Pulsar.Services.Identity.Functions.Application.Commands.Others;

[NoTransaction, RetryOnException(DuplicatedKey = true, VersionConcurrency = true, Retries = 2)]
public class EstabelecimentoModificadoCH : IdentityCommandHandler<AtualizarEstabelecimentoCmd>
{
    public EstabelecimentoModificadoCH(IdentityCommandHandlerContext<AtualizarEstabelecimentoCmd> ctx) : base(ctx)
    {
    }

    protected override async Task HandleAsync(AtualizarEstabelecimentoCmd cmd, CancellationToken ct)
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
                cmd.AuditInfo.ToDomainObject(),
                cmd.TimeStamp);
            await EstabelecimentoRepository.InsertOneAsync(estabelecimento);
        }
        else
        {
            bool editado = estabelecimento.Editar(cmd.Nome, cmd.Cnes, cmd.Redes.Select(r => r.ToObjectId()), cmd.IsAtivo, cmd.AuditInfo.ToDomainObject(), cmd.TimeStamp, out long previousVersion);
            if (editado)
                await EstabelecimentoRepository.ReplaceOneAsync(estabelecimento, previousVersion, ct).CheckModified();
        }
    }
}
