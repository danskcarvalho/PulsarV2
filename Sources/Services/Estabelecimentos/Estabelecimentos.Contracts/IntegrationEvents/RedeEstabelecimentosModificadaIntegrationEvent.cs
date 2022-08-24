using Pulsar.Services.Shared.DTOs;

namespace Pulsar.Services.Estabelecimentos.Contracts.IntegrationEvents;

[EventName("Estabelecimentos:RedeEstabelecimentosModificado")]
public class RedeEstabelecimentosModificadaIntegrationEvent : IntegrationEvent
{
    public string RedeEstabelecimentosId { get; private set; }
    public string DominioId { get; private set; }
    public string Nome { get; private set; }
    public AuditInfoDTO AuditInfo { get; private set; }
    public ChangeEvent Modificacao { get; private set; }

    [JsonConstructor]
    public RedeEstabelecimentosModificadaIntegrationEvent(Guid id, DateTime creationDate, string redeEstabelecimentosId, string dominioId, string nome, AuditInfoDTO auditInfo, ChangeEvent modificacao) : base(id, creationDate, false)
    {
        RedeEstabelecimentosId = redeEstabelecimentosId;
        Nome = nome;
        DominioId = dominioId;
        AuditInfo = auditInfo;
        Modificacao = modificacao;
    }
}
