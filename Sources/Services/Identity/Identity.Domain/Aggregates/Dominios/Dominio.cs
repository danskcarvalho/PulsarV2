﻿using DDD.Contracts;
using Pulsar.BuildingBlocks.Sync.Contracts;
using Pulsar.Services.Identity.Contracts.Shadows;
using Pulsar.Services.Identity.Domain.Aggregates.Usuarios;
using Pulsar.Services.Identity.Domain.Events.Dominios;

namespace Pulsar.Services.Identity.Domain.Aggregates.Dominios;

[TrackChanges(CollectionName = Constants.CollectionNames.DOMINIOS, ShadowType = typeof(DominioShadow))]
public partial class Dominio : AggregateRootWithContext<Dominio>
{
    private string _nome;
    private string _termosBusca;

    [TrackChanges]
    public string Nome
    {
        get => _nome;
        set
        {
            _nome = value;
            if(!IsInitializing)
                _termosBusca = GetTermosBusca();
        }
    }
    public string TermosBusca { get => _termosBusca; private set => _termosBusca = value; }

    [TrackChanges]
    public bool IsAtivo { get; set; }

    [TrackChanges]
    public AuditInfo AuditInfo { get; set; }

    [TrackChanges]
    public ObjectId? UsuarioAdministradorId { get; set; }

    [BsonConstructor]
    public Dominio(ObjectId id, string nome, ObjectId? usuarioAdministradorId, AuditInfo auditInfo) : base(id)
    {
        _nome = nome;
        _termosBusca = GetTermosBusca();
        AuditInfo = auditInfo;
        UsuarioAdministradorId = usuarioAdministradorId;
        IsAtivo = true;
    }
    private string GetTermosBusca()
    {
        return _nome.Tokenize()!;
    }

    public void Esconder(ObjectId usuarioLogadoId)
    {
        this.AuditInfo = this.AuditInfo.EscondidoPor(usuarioLogadoId);
        this.AddDomainEvent(new DominioModificadoDE(usuarioLogadoId, this.Id, this.Nome, this.IsAtivo, this.AuditInfo, this.UsuarioAdministradorId, this.UsuarioAdministradorId, ChangeEvent.Hidden));
    }

    public void Mostrar(ObjectId usuarioLogadoId)
    {
        this.AuditInfo = this.AuditInfo.MostradoPor();
        this.AddDomainEvent(new DominioModificadoDE(usuarioLogadoId, this.Id, this.Nome, this.IsAtivo, this.AuditInfo, this.UsuarioAdministradorId, this.UsuarioAdministradorId, ChangeEvent.Shown));
    }

    public void SetAdministradorDominio(ObjectId usuarioId, ObjectId? editadoPorUsuarioId)
    {
        if (this.UsuarioAdministradorId != null || !this.IsAtivo)
            throw new IdentityDomainException(IdentityExceptionKey.ConviteDominioInvalido);
        this.UsuarioAdministradorId = usuarioId;
        if (editadoPorUsuarioId != null)
            this.AuditInfo = this.AuditInfo.EditadoPor(editadoPorUsuarioId.Value);
    }

    public void Criar(ObjectId usuarioLogadoId, Usuario? usuarioAdministrador)
    {
        if (usuarioAdministrador != null && usuarioAdministrador.IsSuperUsuario)
            throw new IdentityDomainException(IdentityExceptionKey.SuperUsuarioNaoPodeAdministrarDominio);

        this.AddDomainEvent(new DominioModificadoDE(usuarioLogadoId, this.Id, this.Nome, this.IsAtivo, this.AuditInfo, this.UsuarioAdministradorId, null, ChangeEvent.Created));
    }

    public void Editar(ObjectId usuarioLogadoId, string nome, Usuario? usuarioAdministrador, List<ObjectId>? usuariosBloqueados)
    {
        if (usuarioAdministrador != null && usuarioAdministrador.IsSuperUsuario)
            throw new IdentityDomainException(IdentityExceptionKey.SuperUsuarioNaoPodeAdministrarDominio);
        if (usuarioAdministrador != null && usuariosBloqueados != null && usuariosBloqueados.Any(ub => ub == usuarioAdministrador.Id))
            throw new IdentityDomainException(IdentityExceptionKey.UsuarioAdministradorIsBloqueadoDominio);

        var previousAdmin = this.UsuarioAdministradorId;
        this.Nome = nome;
        this.UsuarioAdministradorId = usuarioAdministrador?.Id;
        this.AuditInfo = this.AuditInfo.EditadoPor(usuarioLogadoId);
        this.AddDomainEvent(new DominioModificadoDE(usuarioLogadoId, this.Id, this.Nome, this.IsAtivo, this.AuditInfo, this.UsuarioAdministradorId, previousAdmin, ChangeEvent.Edited));
    }

    public void BloquearOuDesbloquear(ObjectId usuarioLogadoId, bool bloquear)
    {
        this.IsAtivo = !bloquear;
        this.AuditInfo = this.AuditInfo.EditadoPor(usuarioLogadoId);
        this.AddDomainEvent(new DominioModificadoDE(usuarioLogadoId, this.Id, this.Nome, this.IsAtivo, this.AuditInfo, this.UsuarioAdministradorId, this.UsuarioAdministradorId, ChangeEvent.Edited));
    }

    public void BloquearOuDesbloquearUsuarios(ObjectId usuarioLogadoId, List<ObjectId> usuarioIds, bool bloquear)
    {
        if (usuarioIds.Any(u => u == UsuarioAdministradorId))
            throw new IdentityDomainException(IdentityExceptionKey.UsuarioAdministradorNaoPodeSerBloqueadoDominio);
        if (usuarioIds.Any(u => u == Usuario.SuperUsuarioId))
            throw new IdentityDomainException(IdentityExceptionKey.SuperUsuarioNaoPodeSerBloqueadoDominio);
        this.AuditInfo = this.AuditInfo.EditadoPor(usuarioLogadoId);
        this.AddDomainEvent(new UsuariosBloqueadosEmDominioDE(usuarioLogadoId, this.Id, usuarioIds, bloquear));
    }
}
