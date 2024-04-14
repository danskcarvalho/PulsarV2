namespace Pulsar.Services.Facility.Contracts.IntegrationEvents;

[EventName("Facilities:RedeEstabelecimentosModificado")]
public record RedeEstabelecimentosModificadaIE : IntegrationEvent
{
    public required string RedeEstabelecimentosId { get; init; }
    public required string DominioId { get; init; }
    public required string Nome { get; init; }
    public required AuditInfoDTO AuditInfo { get; init; }
    public required ChangeEvent Modificacao { get; init; }
}
