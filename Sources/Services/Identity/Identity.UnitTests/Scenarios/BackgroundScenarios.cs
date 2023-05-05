using MongoDB.Bson;
using Pulsar.BuildingBlocks.DDD;
using Pulsar.Services.Identity.Functions.Application.Functions;

namespace Identity.UnitTests.Scenarios;

public class BackgroundScenarios : IdentityScenarios
{
    [Fact]
    public async Task Atualizar_Rede_Estabelecimentos()
    {
        var redeId = ObjectId.Parse(IdentityDatabase.RedeEstabelecimentosPadraoId);
        var estabelecimentoId = ObjectId.Parse(IdentityDatabase.EstabelecimentoPadraoId);
        var sid = ObjectId.Parse(IdentityDatabase.AdminUserId);
        var fn = CreateFunction<AtualizarRedeEstabelecimentosFN>();
        var auditInfo = new AuditInfo(sid).EditadoPor(sid);
        await fn.Run(new Pulsar.Services.Estabelecimentos.Contracts.IntegrationEvents.RedeEstabelecimentosModificadaIE()
        {
            RedeEstabelecimentosId = redeId.ToString(),
            AuditInfo = auditInfo.ToDTO(),
            DominioId = IdentityDatabase.DominioPadraoId,
            Nome = "NOVO NOME",
            Modificacao = Pulsar.Services.Shared.Enumerations.ChangeEvent.Edited
        });

        var rede = await RedesEstabelecimentos.FindAsync(x => x.Id == redeId).FirstOrDefaultAsync();
        var estabelecimento = await Estabelecimentos.FindAsync(x => x.Id == estabelecimentoId).FirstOrDefaultAsync();
        Assert.NotNull(rede);
        Assert.NotNull(estabelecimento);
        Assert.Equal("NOVO NOME", rede.Nome);
        Assert.Null(rede.AuditInfo.RemovidoEm);
        Assert.Contains(redeId, estabelecimento.Redes);

        await fn.Run(new Pulsar.Services.Estabelecimentos.Contracts.IntegrationEvents.RedeEstabelecimentosModificadaIE()
        {
            RedeEstabelecimentosId = redeId.ToString(),
            AuditInfo = auditInfo.RemovidoPor(sid).ToDTO(),
            DominioId = IdentityDatabase.DominioPadraoId,
            Nome = "NOVO NOME",
            Modificacao = Pulsar.Services.Shared.Enumerations.ChangeEvent.Deleted
        });
        rede = await RedesEstabelecimentos.FindAsync(x => x.Id == redeId).FirstOrDefaultAsync();
        Assert.NotNull(rede);
        Assert.NotNull(rede.AuditInfo.RemovidoEm);

        estabelecimento = await Estabelecimentos.FindAsync(x => x.Id == estabelecimentoId).FirstOrDefaultAsync();
        Assert.NotNull(estabelecimento);
        Assert.DoesNotContain(redeId, estabelecimento.Redes);
    }

    [Fact]
    public async Task Atualizar_Estabelecimentos()
    {
        var estabelecimentoId = ObjectId.Parse(IdentityDatabase.EstabelecimentoPadraoId);
        var estabelecimento = await Estabelecimentos.FindAsync(x => x.Id == estabelecimentoId).FirstOrDefaultAsync();
        Assert.NotNull(estabelecimento);
        Assert.True(estabelecimento.IsAtivo);
        Assert.Equal("PADRÃO", estabelecimento.Nome);
        Assert.Equal("00112233", estabelecimento.Cnes);
        Assert.Single(estabelecimento.Redes);

        var sid = ObjectId.Parse(IdentityDatabase.AdminUserId);
        var fn = CreateFunction<AtualizarEstabelecimentoFN>();
        var auditInfo = new AuditInfo(sid).EditadoPor(sid);
        await fn.Run(new Pulsar.Services.Estabelecimentos.Contracts.IntegrationEvents.EstabelecimentoModificadoIE()
        {
            AuditInfo = auditInfo.ToDTO(),
            DominioId = IdentityDatabase.DominioPadraoId,
            Nome = "NOVO NOME",
            Modificacao = Pulsar.Services.Shared.Enumerations.ChangeEvent.Edited,
            IsAtivo = false,
            Cnes = "1498793",
            EstabelecimentoId = estabelecimentoId.ToString(),
            Redes = new List<string> { IdentityDatabase.RedeEstabelecimentosPadraoId, IdentityDatabase.RedeEstabelecimentosNaoPadraoId }
        });

        estabelecimento = await Estabelecimentos.FindAsync(x => x.Id == estabelecimentoId).FirstOrDefaultAsync();
        Assert.NotNull(estabelecimento);
        Assert.False(estabelecimento.IsAtivo);
        Assert.Equal("NOVO NOME", estabelecimento.Nome);
        Assert.Equal("1498793", estabelecimento.Cnes);
        Assert.Equal(2, estabelecimento.Redes.Count);
    }
}
