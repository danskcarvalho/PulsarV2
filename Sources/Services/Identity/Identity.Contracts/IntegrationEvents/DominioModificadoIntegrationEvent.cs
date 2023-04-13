using Pulsar.BuildingBlocks.EventBus.Contracts;
using Pulsar.BuildingBlocks.EventBus.Events;
using Pulsar.Services.Shared.DTOs;
using Pulsar.Services.Shared.Enumerations;
using System;

namespace Pulsar.Services.Identity.Contracts.IntegrationEvents;

[EventName("Identity:DominioModificado")]
public record DominioModificadoIntegrationEvent : IntegrationEvent
{
    public required string DominioId { get; init; }
    public required string Nome { get; init; }
    public required bool IsAtivo { get; init; }
    public required AuditInfoDTO AuditInfo { get; init; }
    public required string? UsuarioAdministradorId { get; init; }
    public required string? UsuarioAdministradorAnteriorId { get; init; }
    public required ChangeEvent Modificacao { get; init; }
}
