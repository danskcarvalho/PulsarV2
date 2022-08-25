using Pulsar.BuildingBlocks.EventBus.Contracts;
using Pulsar.BuildingBlocks.EventBus.Events;
using Pulsar.Services.Shared.DTOs;
using Pulsar.Services.Shared.Enumerations;
using System;

namespace Pulsar.Services.Identity.Contracts.IntegrationEvents;

[EventName("Identity:DominioModificado")]
public class DominioModificadoIntegrationEvent : IntegrationEvent
{
    public string DominioId { get; set; }
    public string Nome { get; set; }
    public bool IsAtivo { get; set; }
    public AuditInfoDTO AuditInfo { get; set; }
    public string? UsuarioAdministradorId { get; set; }
    public string? UsuarioAdministradorAnteriorId { get; set; }
    public ChangeEvent Modificacao { get; set; }

    public DominioModificadoIntegrationEvent(Guid id, DateTime creationDate, string dominioId, string nome, bool isAtivo, AuditInfoDTO auditInfo, string? usuarioAdministradorId, string? usuarioAdministradorAnteriorId, 
        ChangeEvent modificacao) : base(id, creationDate)
    {
        DominioId = dominioId;
        Nome = nome;
        IsAtivo = isAtivo;
        AuditInfo = auditInfo;
        UsuarioAdministradorId = usuarioAdministradorId;
        UsuarioAdministradorAnteriorId = usuarioAdministradorAnteriorId;
        Modificacao = modificacao;
    }
}
