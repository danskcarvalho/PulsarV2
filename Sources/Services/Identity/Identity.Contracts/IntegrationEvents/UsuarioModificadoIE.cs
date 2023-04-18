using Pulsar.BuildingBlocks.EventBus.Contracts;
using Pulsar.BuildingBlocks.EventBus.Events;
using Pulsar.Services.Shared.DTOs;
using Pulsar.Services.Shared.Enumerations;
using System;

namespace Pulsar.Services.Identity.Contracts.IntegrationEvents;

[EventName("Identity:UsuarioModificado")]
public record UsuarioModificadoIE : IntegrationEvent
{
    public required string UsuarioId { get; init; }
    public required string? AvatarUrl { get; init; }
    public required string PrimeiroNome { get; init; }
    public required string? UltimoNome { get; init; }
    public required string NomeCompleto { get; init; }
    public required string? Email { get; init; }
    public required bool IsAtivo { get; init; }
    public required string NomeUsuario { get; init; }
    public required bool IsConvitePendente { get; init; }
    public required AuditInfoDTO AuditInfo { get; init; }
    public required ChangeEvent Modificacao { get; init; }
}
