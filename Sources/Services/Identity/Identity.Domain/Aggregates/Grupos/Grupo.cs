﻿using DDD.Contracts;
using Pulsar.BuildingBlocks.DDD;
using Pulsar.Services.Identity.Contracts.Commands.Grupos;
using Pulsar.Services.Identity.Domain.Aggregates.Usuarios;
using Pulsar.Services.Identity.Domain.Events.Grupos;

namespace Pulsar.Services.Identity.Domain.Aggregates.Grupos;

public partial class Grupo : AggregateRootWithContext<Grupo>
{
    private string _termosBusca;
    private string _nome;
    private const int MAX_NUM_USUARIOS = 5000; // -- 5000 usuários
    private const int MAX_NUM_SUBGRUPOS = 100; // -- 100 grupos

    public ObjectId DominioId { get; private set; }
    public string Nome
    {
        get => _nome; private set
        {
            _nome = value;
            if(!IsInitializing)
                _termosBusca = GetTermosBusca();
        }
    }
    public List<SubGrupo> SubGrupos { get; private set; }
    public AuditInfo AuditInfo { get; set; }
    public string TermosBusca
    {
        get => _termosBusca;
        private set => _termosBusca = value;
    }
    public int NumSubGrupos { get; set; }
    public int NumUsuarios { get; set; }


    [BsonConstructor(nameof(Id), nameof(DominioId), nameof(Nome), nameof(AuditInfo), nameof(SubGrupos))]
    public Grupo(ObjectId id, ObjectId dominioId, string nome, AuditInfo auditInfo, IEnumerable<SubGrupo>? subGrupos = null) : base(id)
    {
        _nome = nome;
        SubGrupos = subGrupos != null ? new List<SubGrupo>(subGrupos) : new List<SubGrupo>();
        AuditInfo = auditInfo;
        DominioId = dominioId;
        _termosBusca = GetTermosBusca();
    }

    private string GetTermosBusca()
    {
        return Nome.Tokenize()!;
    }

    public void Criar()
    {
        this.AddDomainEvent(new GrupoModificadoDE(this.AuditInfo.CriadoPorUsuarioId!.Value, this.DominioId, this.Id, this.Nome, this.AuditInfo, new List<SubGrupo>(), new List<SubGrupo>(), new List<SubGrupo>(), ChangeEvent.Created, Version));
    }

    public void Remover(ObjectId usuarioLogadoId)
    {
        this.AuditInfo = this.AuditInfo.RemovidoPor(usuarioLogadoId);
        this.AddDomainEvent(new GrupoModificadoDE(usuarioLogadoId, this.DominioId, this.Id, this.Nome, this.AuditInfo, new List<SubGrupo>(), new List<SubGrupo>(), new List<SubGrupo>(), ChangeEvent.Deleted, Version));
    }

    public void Editar(ObjectId usuarioLogadoId, string nome)
    {
        this.AuditInfo = this.AuditInfo.EditadoPor(usuarioLogadoId);
        this.Nome = nome;
        this.AddDomainEvent(new GrupoModificadoDE(usuarioLogadoId, this.DominioId, this.Id, this.Nome, this.AuditInfo, new List<SubGrupo>(), new List<SubGrupo>(), new List<SubGrupo>(), ChangeEvent.Edited, Version)); 
    }

    public ObjectId CriarSubGrupo(ObjectId usuarioLogadoId, string nome, List<PermissoesDominio> permissoesDominio, List<CriarSubGrupoCmd.PermissoesEstabelecimentoOuRede> permissoesEstabelecimento)
    {
        nome = nome.Trim();
        if (SubGrupos.Any(sg => string.Compare(sg.Nome, nome, true) == 0))
            throw new IdentityDomainException(IdentityExceptionKey.SubgrupoJaExistente);
        this.AuditInfo = this.AuditInfo.EditadoPor(usuarioLogadoId);
        var subgrupo = new SubGrupo(ObjectId.GenerateNewId(), nome, permissoesDominio, 
            permissoesEstabelecimento.Select(pe => new SubGrupoPermissoesEstabelecimento(new Seletor(pe.EstabelecimentoId?.ToObjectId(), pe.RedeEstabelecimentosId?.ToObjectId()), pe.Permissoes!)));
        this.SubGrupos.Add(subgrupo);
        this.NumSubGrupos = this.SubGrupos.Count;

        if (this.NumSubGrupos > Grupo.MAX_NUM_SUBGRUPOS)
            throw new IdentityDomainException(IdentityExceptionKey.NumSubgruposExcedeMaximo);

        this.AddDomainEvent(new GrupoModificadoDE(usuarioLogadoId, this.DominioId, this.Id, this.Nome, this.AuditInfo, new List<SubGrupo>(),new List<SubGrupo> { subgrupo }, new List<SubGrupo>(), ChangeEvent.Edited, Version));
        return subgrupo.SubGrupoId;
    }

    public void EditarSubGrupo(ObjectId usuarioLogadoId, ObjectId subgrupoId, string nome, List<PermissoesDominio> permissoesDominios, List<EditarSubGrupoCmd.PermissoesEstabelecimentoOuRede> permissoesEstabelecimentoOuRedes)
    {
        this.AuditInfo = this.AuditInfo.EditadoPor(usuarioLogadoId);
        var subgrupo = SubGrupos.FirstOrDefault(sg => sg.SubGrupoId == subgrupoId);
        if (subgrupo == null)
            throw new IdentityDomainException(IdentityExceptionKey.SubgrupoNaoEncontrado);
        subgrupo.Editar(nome, permissoesDominios, permissoesEstabelecimentoOuRedes);
        this.AddDomainEvent(new GrupoModificadoDE(usuarioLogadoId, this.DominioId, this.Id, this.Nome, this.AuditInfo, new List<SubGrupo> { subgrupo }, new List<SubGrupo>(), new List<SubGrupo>(), ChangeEvent.Edited, Version));
    }

    public void RemoverSubGrupo(ObjectId usuarioLogadoId, ObjectId subgrupoId)
    {
        this.AuditInfo = this.AuditInfo.EditadoPor(usuarioLogadoId);
        var subgrupo = SubGrupos.FirstOrDefault(sg => sg.SubGrupoId == subgrupoId);
        if (subgrupo == null)
            throw new IdentityDomainException(IdentityExceptionKey.SubgrupoNaoEncontrado);
        SubGrupos.Remove(subgrupo);
        this.NumSubGrupos = this.SubGrupos.Count;

        if (this.NumSubGrupos > Grupo.MAX_NUM_SUBGRUPOS)
            throw new IdentityDomainException(IdentityExceptionKey.NumSubgruposExcedeMaximo);

        this.AddDomainEvent(new GrupoModificadoDE(usuarioLogadoId, this.DominioId, this.Id, this.Nome, this.AuditInfo, new List<SubGrupo>(), new List<SubGrupo>(), new List<SubGrupo> { subgrupo }, ChangeEvent.Edited, Version));
    }

    public void AdicionarUsuariosEmSubGrupo(ObjectId usuarioLogadoId, ObjectId subgrupoId, List<ObjectId> usuarioIds)
    {
        if (!SubGrupos.Any(sg => sg.SubGrupoId == subgrupoId))
            throw new IdentityDomainException(IdentityExceptionKey.SubgrupoNaoEncontrado);
        if (usuarioIds.Any(u => u == Usuario.SuperUsuarioId))
            throw new IdentityDomainException(IdentityExceptionKey.SuperUsuarioNaoPodeserAdicionadoEmGrupo);

        var subgrupo = SubGrupos.First(sg => sg.SubGrupoId == subgrupoId);
        subgrupo.UsuarioIds.UnionWith(usuarioIds);
        subgrupo.NumUsuarios = subgrupo.UsuarioIds.Count;
        this.NumUsuarios = this.SubGrupos.Sum(sg => sg.NumUsuarios);

        if (this.NumUsuarios > Grupo.MAX_NUM_USUARIOS)
            throw new IdentityDomainException(IdentityExceptionKey.NumUsuariosGrupoExcedeMaximo);

        this.AuditInfo = this.AuditInfo.EditadoPor(usuarioLogadoId);
        this.AddDomainEvent(new GrupoModificadoDE(usuarioLogadoId, this.DominioId, this.Id, this.Nome, this.AuditInfo, new List<SubGrupo> { subgrupo }, new List<SubGrupo>(), new List<SubGrupo>(), ChangeEvent.Edited, Version));
    }

    public void RemoverUsuariosEmSubGrupo(ObjectId usuarioLogadoId, ObjectId subgrupoId, List<ObjectId> usuarioIds)
    {
        if (!SubGrupos.Any(sg => sg.SubGrupoId == subgrupoId))
            throw new IdentityDomainException(IdentityExceptionKey.SubgrupoNaoEncontrado);
        if (usuarioIds.Any(u => u == Usuario.SuperUsuarioId))
            throw new IdentityDomainException(IdentityExceptionKey.SuperUsuarioNaoPodeserAdicionadoEmGrupo);

        var subgrupo = SubGrupos.First(sg => sg.SubGrupoId == subgrupoId);
        subgrupo.UsuarioIds.ExceptWith(usuarioIds);
        subgrupo.NumUsuarios = subgrupo.UsuarioIds.Count;
        this.NumUsuarios = this.SubGrupos.Sum(sg => sg.NumUsuarios);

        if (this.NumUsuarios > Grupo.MAX_NUM_USUARIOS)
            throw new IdentityDomainException(IdentityExceptionKey.NumUsuariosGrupoExcedeMaximo);

        this.AuditInfo = this.AuditInfo.EditadoPor(usuarioLogadoId);
        this.AddDomainEvent(new GrupoModificadoDE(usuarioLogadoId, this.DominioId, this.Id, this.Nome, this.AuditInfo, new List<SubGrupo> { subgrupo }, new List<SubGrupo>(), new List<SubGrupo>(), ChangeEvent.Edited, Version));
    }
}
