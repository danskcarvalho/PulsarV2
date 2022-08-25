using Pulsar.BuildingBlocks.EventBus.Contracts;
using Pulsar.BuildingBlocks.EventBus.Events;
using Pulsar.Services.Shared.DTOs;
using Pulsar.Services.Shared.Enumerations;
using System;

namespace Pulsar.Services.Identity.Contracts.IntegrationEvents;

[EventName("Identity:UsuarioModificado")]
public class UsuarioModificadoIntegrationEvent : IntegrationEvent
{
    public string UsuarioId { get; set; }
    public string? PublicAvatarUrl { get; set; }
    public string? PrivateAvatarUrl { get; set; }
    public string PrimeiroNome { get; set; }
    public string? UltimoNome { get; set; }
    public string NomeCompleto { get; set; }
    public bool IsAtivo { get; set; }
    public string NomeUsuario { get; set; }
    public bool IsConvitePendente { get; set; }
    public AuditInfoDTO AuditInfo { get; set; }
    public ChangeEvent Modificacao { get; set; }

    public UsuarioModificadoIntegrationEvent(Guid id, DateTime creationDate, string usuarioId, string? publicAvatarUrl, string? privateAvatarUrl, string primeiroNome, string? ultimoNome, string nomeCompleto, bool isAtivo, string nomeUsuario,
        bool isConvitePendente, AuditInfoDTO auditInfo, ChangeEvent modificacao) : base(id, creationDate, false)
    {
        UsuarioId = usuarioId;
        PublicAvatarUrl = publicAvatarUrl;
        PrivateAvatarUrl = privateAvatarUrl;
        PrimeiroNome = primeiroNome;
        UltimoNome = ultimoNome;
        NomeCompleto = nomeCompleto;
        IsAtivo = isAtivo;
        NomeUsuario = nomeUsuario;
        IsConvitePendente = isConvitePendente;
        AuditInfo = auditInfo;
        Modificacao = modificacao;
    }
}
