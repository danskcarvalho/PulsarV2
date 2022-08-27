using Pulsar.Services.Identity.Contracts.Commands.Grupos;
using Pulsar.Services.Identity.Domain.Aggregates.Usuarios;
using Pulsar.Services.Identity.Domain.Events.Grupos;

namespace Pulsar.Services.Identity.Domain.Aggregates.Grupos;

public class Grupo : AggregateRoot
{
    private string _termosBusca;
    private string _nome;

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
        this.AddDomainEvent(new GrupoModificadoDomainEvent(this.AuditInfo.CriadoPorUsuarioId!.Value, this.Id, this.Nome, this.AuditInfo, new List<SubGrupo>(), new List<SubGrupo>(), new List<SubGrupo>(), ChangeEvent.Created, Version));
    }

    public void Remover(ObjectId usuarioLogadoId)
    {
        this.AuditInfo = this.AuditInfo.RemovidoPor(usuarioLogadoId);
        this.Version++;
        this.AddDomainEvent(new GrupoModificadoDomainEvent(usuarioLogadoId, this.Id, this.Nome, this.AuditInfo, new List<SubGrupo>(), new List<SubGrupo>(), new List<SubGrupo>(), ChangeEvent.Deleted, Version));
    }

    public void Editar(ObjectId usuarioLogadoId, string nome)
    {
        this.AuditInfo = this.AuditInfo.EditadoPor(usuarioLogadoId);
        this.Nome = nome;
        this.Version++;
        this.AddDomainEvent(new GrupoModificadoDomainEvent(usuarioLogadoId, this.Id, this.Nome, this.AuditInfo, new List<SubGrupo>(), new List<SubGrupo>(), new List<SubGrupo>(), ChangeEvent.Deleted, Version));
    }

    public void AtualizarNumUsuarios(ObjectId usuarioLogadoId, int deltaNumUsuarios)
    {
        this.AddDomainEvent(new NumUsuariosEmGrupoModificadoDomainEvent(usuarioLogadoId, this.Id, deltaNumUsuarios));
    }

    public ObjectId CriarSubGrupo(ObjectId usuarioLogadoId, string nome, List<PermissoesDominio> permissoesDominio, List<CriarSubGrupoCommand.PermissoesEstabelecimentoOuRede> permissoesEstabelecimento)
    {
        nome = nome.Trim();
        if (SubGrupos.Any(sg => string.Compare(sg.Nome, nome, true) == 0))
            throw new IdentityDomainException(ExceptionKey.SubgrupoJaExistente);
        this.AuditInfo = this.AuditInfo.EditadoPor(usuarioLogadoId);
        this.Version++;
        var subgrupo = new SubGrupo(ObjectId.GenerateNewId(), nome, permissoesDominio, 
            permissoesEstabelecimento.Select(pe => new SubGrupoPermissoesEstabelecimento(new Seletor(pe.EstabelecimentoId?.ToObjectId(), pe.RedeEstabelecimentosId?.ToObjectId()), pe.Permissoes!)));
        this.AddDomainEvent(new GrupoModificadoDomainEvent(usuarioLogadoId, this.Id, this.Nome, this.AuditInfo, new List<SubGrupo>(),new List<SubGrupo> { subgrupo }, new List<SubGrupo>(), ChangeEvent.Edited, Version));
        return subgrupo.SubGrupoId;
    }

    public void EditarSubGrupo(ObjectId usuarioLogadoId, ObjectId subgrupoId, string nome, List<PermissoesDominio> permissoesDominios, List<EditarSubGrupoCommand.PermissoesEstabelecimentoOuRede> permissoesEstabelecimentoOuRedes)
    {
        this.AuditInfo = this.AuditInfo.EditadoPor(usuarioLogadoId);
        this.Version++;
        var subgrupo = SubGrupos.FirstOrDefault(sg => sg.SubGrupoId == subgrupoId);
        if (subgrupo == null)
            throw new IdentityDomainException(ExceptionKey.SubgrupoNaoEncontrado);
        subgrupo.Editar(nome, permissoesDominios, permissoesEstabelecimentoOuRedes);
        this.AddDomainEvent(new GrupoModificadoDomainEvent(usuarioLogadoId, this.Id, this.Nome, this.AuditInfo, new List<SubGrupo> { subgrupo }, new List<SubGrupo>(), new List<SubGrupo>(), ChangeEvent.Edited, Version));
    }

    public void RemoverSubGrupo(ObjectId usuarioLogadoId, ObjectId subgrupoId)
    {
        this.AuditInfo = this.AuditInfo.EditadoPor(usuarioLogadoId);
        this.Version++;
        var subgrupo = SubGrupos.FirstOrDefault(sg => sg.SubGrupoId == subgrupoId);
        if (subgrupo == null)
            throw new IdentityDomainException(ExceptionKey.SubgrupoNaoEncontrado);
        SubGrupos.Remove(subgrupo);
        this.AddDomainEvent(new GrupoModificadoDomainEvent(usuarioLogadoId, this.Id, this.Nome, this.AuditInfo, new List<SubGrupo>(), new List<SubGrupo>(), new List<SubGrupo> { subgrupo }, ChangeEvent.Edited, Version));
    }

    public void AdicionarUsuariosEmSubGrupo(ObjectId usuarioLogadoId, ObjectId subgrupoId, List<ObjectId> usuarioIds)
    {
        if (!SubGrupos.Any(sg => sg.SubGrupoId == subgrupoId))
            throw new IdentityDomainException(ExceptionKey.SubgrupoNaoEncontrado);
        if (usuarioIds.Any(u => u == Usuario.SuperUsuarioId))
            throw new IdentityDomainException(ExceptionKey.SuperUsuarioNaoPodeserAdicionadoEmGrupo);

        this.AddDomainEvent(new UsuariosAdicionadosOuRemovidosEmGrupoDomainEvent(usuarioLogadoId, this.Id, subgrupoId, this.DominioId, false, usuarioIds));
    }

    public void RemoverUsuariosEmSubGrupo(ObjectId usuarioLogadoId, ObjectId subgrupoId, List<ObjectId> usuarioIds)
    {
        if (!SubGrupos.Any(sg => sg.SubGrupoId == subgrupoId))
            throw new IdentityDomainException(ExceptionKey.SubgrupoNaoEncontrado);
        if (usuarioIds.Any(u => u == Usuario.SuperUsuarioId))
            throw new IdentityDomainException(ExceptionKey.SuperUsuarioNaoPodeserAdicionadoEmGrupo);

        this.AddDomainEvent(new UsuariosAdicionadosOuRemovidosEmGrupoDomainEvent(usuarioLogadoId, this.Id, subgrupoId, this.DominioId, true, usuarioIds));
    }
}
