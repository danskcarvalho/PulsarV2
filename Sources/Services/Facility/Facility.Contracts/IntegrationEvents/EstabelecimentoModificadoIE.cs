﻿namespace Pulsar.Services.Facility.Contracts.IntegrationEvents;

[EventName("Facilities:EstabelecimentoModificado")]
public record EstabelecimentoModificadoIE : IntegrationEvent
{
    public required string EstabelecimentoId { get; init; }
    public required string DominioId { get; init; }
    public required string Nome { get; init; }
    public required string Cnes { get; init; }
    public required List<string> Redes { get; init; }
    public required bool IsAtivo { get; init; }
    public required AuditInfoDTO AuditInfo { get; init; }
    public required ChangeEvent Modificacao { get; init; }
}
