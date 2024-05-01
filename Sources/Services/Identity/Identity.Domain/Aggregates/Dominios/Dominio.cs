using Pulsar.BuildingBlocks.DDD;
using Pulsar.BuildingBlocks.Sync.Contracts;
using Pulsar.Services.Identity.Contracts.Shadows;
using Pulsar.Services.Identity.Domain.Aggregates.Convites;
using Pulsar.Services.Identity.Domain.Aggregates.Grupos;
using Pulsar.Services.Identity.Domain.Aggregates.Usuarios;
using Pulsar.Services.Identity.Domain.Events.Dominios;
using Pulsar.Services.Identity.Domain.Events.Grupos;
using Pulsar.Services.Identity.Domain.Events.Usuarios;
using static Pulsar.Services.Identity.Contracts.DTOs.UsuarioDetalhesDTO;

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

    public async Task Esconder(ObjectId usuarioLogadoId)
    {
        this.AuditInfo = this.AuditInfo.EscondidoPor(usuarioLogadoId);
        this.AddDomainEvent(new DominioModificadoDE(usuarioLogadoId, this.Id, this.Nome, this.IsAtivo, this.AuditInfo, this.UsuarioAdministradorId, this.UsuarioAdministradorId, ChangeEvent.Hidden));
        await this.Replace();
    }

    public async Task Mostrar(ObjectId usuarioLogadoId)
    {
        this.AuditInfo = this.AuditInfo.MostradoPor();
        this.AddDomainEvent(new DominioModificadoDE(usuarioLogadoId, this.Id, this.Nome, this.IsAtivo, this.AuditInfo, this.UsuarioAdministradorId, this.UsuarioAdministradorId, ChangeEvent.Shown));
        await this.Replace();
    }

    public void SetAdministradorDominio(ObjectId usuarioId, ObjectId? editadoPorUsuarioId)
    {
        if (this.UsuarioAdministradorId != null || !this.IsAtivo)
            throw new IdentityDomainException(ExceptionKey.ConviteDominioInvalido);
        this.UsuarioAdministradorId = usuarioId;
        if (editadoPorUsuarioId != null)
            this.AuditInfo = this.AuditInfo.EditadoPor(editadoPorUsuarioId.Value);
    }

    public async Task Criar(ObjectId usuarioLogadoId, Usuario? usuarioAdministrador)
    {
        if (usuarioAdministrador != null && usuarioAdministrador.IsSuperUsuario)
            throw new IdentityDomainException(ExceptionKey.SuperUsuarioNaoPodeAdministrarDominio);

        this.AddDomainEvent(new DominioModificadoDE(usuarioLogadoId, this.Id, this.Nome, this.IsAtivo, this.AuditInfo, this.UsuarioAdministradorId, null, ChangeEvent.Created));
        await this.Insert();
    }

    public async Task Editar(ObjectId usuarioLogadoId, string nome, Usuario? usuarioAdministrador, List<ObjectId>? usuariosBloqueados)
    {
        if (usuarioAdministrador != null && usuarioAdministrador.IsSuperUsuario)
            throw new IdentityDomainException(ExceptionKey.SuperUsuarioNaoPodeAdministrarDominio);
        if (usuarioAdministrador != null && usuariosBloqueados != null && usuariosBloqueados.Any(ub => ub == usuarioAdministrador.Id))
            throw new IdentityDomainException(ExceptionKey.UsuarioAdministradorIsBloqueadoDominio);

        var previousAdmin = this.UsuarioAdministradorId;
        this.Nome = nome;
        this.UsuarioAdministradorId = usuarioAdministrador?.Id;
        this.AuditInfo = this.AuditInfo.EditadoPor(usuarioLogadoId);
        this.AddDomainEvent(new DominioModificadoDE(usuarioLogadoId, this.Id, this.Nome, this.IsAtivo, this.AuditInfo, this.UsuarioAdministradorId, previousAdmin, ChangeEvent.Edited));
        await this.Replace();
    }

    public async Task BloquearOuDesbloquear(ObjectId usuarioLogadoId, bool bloquear)
    {
        this.IsAtivo = !bloquear;
        this.AuditInfo = this.AuditInfo.EditadoPor(usuarioLogadoId);
        this.AddDomainEvent(new DominioModificadoDE(usuarioLogadoId, this.Id, this.Nome, this.IsAtivo, this.AuditInfo, this.UsuarioAdministradorId, this.UsuarioAdministradorId, ChangeEvent.Edited));
        await this.Replace();
    }

    public async Task BloquearOuDesbloquearUsuarios(ObjectId usuarioLogadoId, List<ObjectId> usuarioIds, bool bloquear)
    {
        if (usuarioIds.Any(u => u == UsuarioAdministradorId))
            throw new IdentityDomainException(ExceptionKey.UsuarioAdministradorNaoPodeSerBloqueadoDominio);
        if (usuarioIds.Any(u => u == Usuario.SuperUsuarioId))
            throw new IdentityDomainException(ExceptionKey.SuperUsuarioNaoPodeSerBloqueadoDominio);
        this.AuditInfo = this.AuditInfo.EditadoPor(usuarioLogadoId);
        this.AddDomainEvent(new UsuariosBloqueadosEmDominioDE(usuarioLogadoId, this.Id, usuarioIds, bloquear));
        await this.Replace();
    }
}
