namespace Pulsar.Services.Estabelecimentos.Contracts.IntegrationEvents;

public class RedeEstabelecimentosEditadaOuCriadaIntegrationEvent : IntegrationEvent
{
    public string RedeEstabelecimentosId { get; private set; }
    public string DominioId { get; private set; }
    public string Nome { get; private set; }
    public AuditInfoDTO AuditInfo { get; private set; }

    [JsonConstructor]
    public RedeEstabelecimentosEditadaOuCriadaIntegrationEvent(Guid id, DateTime creationDate, string redeEstabelecimentosId, string dominioId, string nome, AuditInfoDTO auditInfo) : base(id, creationDate, false)
    {
        RedeEstabelecimentosId = redeEstabelecimentosId;
        Nome = nome;
        DominioId = dominioId;
        AuditInfo = auditInfo;
    }
}
