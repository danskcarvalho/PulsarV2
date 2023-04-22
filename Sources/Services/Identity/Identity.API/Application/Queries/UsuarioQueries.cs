using Pulsar.BuildingBlocks.Utils.Bson;
using Pulsar.Services.Identity.API.Application.BaseTypes;
using Pulsar.Services.Identity.Domain.Aggregates.Grupos;
using Pulsar.Services.Identity.Domain.Aggregates.Usuarios;
using Pulsar.Services.Shared.DTOs;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Pulsar.Services.Identity.API.Application.Queries;

public partial class UsuarioQueries : IdentityQueries, IUsuarioQueries
{
    public UsuarioQueries(IdentityQueriesContext ctx) : base(ctx)
    {
    }

    public async Task<PaginatedListDTO<UsuarioListadoDTO>> FindUsuarios(UsuarioFiltroDTO filtro)
    {
        filtro.Filtro = filtro.Filtro?.ToLowerInvariant().Trim();
        return await this.StartCausallyConsistentSectionAsync(async ct =>
        {
            var projection = Builders<Usuario>.Projection.Expression(x => new UsuarioListadoDTO(x.Id.ToString(), x.Email!, x.PrimeiroNome, x.NomeCompleto, x.NomeUsuario)
            {
                AvatarUrl = x.AvatarUrl,
                IsAtivo = x.IsAtivo,
                IsConvitePendente = x.IsConvitePendente,
                UltimoNome = x.UltimoNome
            });
            var (usuarios, next) = await UsuariosCollection.Paginated(filtro.Limit ?? 50, filtro.Cursor, new { filtro.Filtro }).FindAsync<CursorUsuarioListado, UsuarioListadoDTO>(projection,
            c =>
            {
                var textSearch = !IsEmail(c.Filtro) ? c.Filtro.ToTextSearch<Usuario>() : Filters.Usuarios.Create(f => f.Eq(u => u.Email, c.Filtro));
                return Filters.Usuarios.Create(f => f.And(textSearch, f.Ne(u => u.Email, null)));
            });
            return new PaginatedListDTO<UsuarioListadoDTO>(usuarios, next);
        }, filtro.ConsistencyToken);

        bool IsEmail(string? filtro)
        {
            return filtro?.Contains('@') == true;
        }
    }

    public async Task<BasicUserInfoDTO?> GetBasicUserInfo(string usuarioId, string? consistencyToken)
    {
        return await this.StartCausallyConsistentSectionAsync(async ct =>
        {
            var id = ObjectId.Parse(usuarioId);
            var usuario = await UsuariosCollection.FindAsync(u => u.Id == id).FirstOrDefaultAsync();
            if (usuario == null)
                return null;
            return new BasicUserInfoDTO(usuario.Id.ToString(), usuario.PrimeiroNome, usuario.UltimoNome, usuario.NomeUsuario, usuario.Email, usuario.NomeUsuario, usuario.AvatarUrl, usuario.IsSuperUsuario);
        }, consistencyToken);
    }

    public async Task<List<BasicUserInfoDTO>> GetBasicUsersInfo(IEnumerable<string> usuarioIds, string? consistencyToken)
    {
        if (usuarioIds.Count() == 0)
            return new List<BasicUserInfoDTO>();
        var r = await this.StartCausallyConsistentSectionAsync(async ct =>
        {
            var allIds = usuarioIds.Select(x => x.ToObjectId()).ToList();
            var usuarios = await UsuariosCollection.FindAsync(u => allIds.Contains(u.Id)).ToListAsync();

            return usuarios.Select(u => new BasicUserInfoDTO(u.Id.ToString(), u.PrimeiroNome, u.UltimoNome, u.NomeUsuario, u.Email, u.NomeUsuario, u.AvatarUrl, u.IsSuperUsuario));
        }, consistencyToken);
        return r.ToList();
    }

    public async Task<List<UsuarioDetalhesDTO>> GetUsuarioDetalhes(IEnumerable<string> usuarioIds, string? consistencyToken)
    {
        if (usuarioIds.Count() == 0)
            return new List<UsuarioDetalhesDTO>();
        return await this.StartCausallyConsistentSectionAsync(async ct =>
        {
            var allIds = usuarioIds.Select(x => x.ToObjectId()).ToList();
            var usuarios = await UsuariosCollection.FindAsync(u => allIds.Contains(u.Id)).ToListAsync();
            var grupos = await GruposCollection.FindAsync(Filters.Grupos.Create(f => 
                f.And(
                    f.Eq(g => g.AuditInfo.RemovidoEm, null), 
                    f.ElemMatch(g => g.SubGrupos, Builders<SubGrupo>.Filter.AnyIn(sg => sg.UsuarioIds, allIds))))).ToListAsync();
            var allDominioIds = usuarios.SelectMany(u => u.DominiosAdministrados).Union(usuarios.SelectMany(u => u.DominiosBloqueados))
                .Union(grupos.Select(g => g.DominioId)).ToList();
            var dominios = (await DominiosCollection.FindAsync(d => allDominioIds.Contains(d.Id)).ToListAsync()).MapByUnique(d => d.Id);

            return usuarios.Select(u => new UsuarioDetalhesDTO(
                    u.Id.ToString(),
                    u.AvatarUrl,
                    u.PrimeiroNome,
                    u.UltimoNome,
                    u.NomeCompleto,
                    grupos
                        .SelectMany(g => g.SubGrupos.Select(sg => (Grupo: g, SubGrupo: sg)))
                        .Where(g => g.SubGrupo.UsuarioIds.Contains(u.Id))
                        .Select(g => new UsuarioDetalhesDTO.UsuarioGrupo(
                            g.Grupo.DominioId.ToString(), dominios[g.Grupo.DominioId].Nome,
                            g.Grupo.Id.ToString(), g.Grupo.Nome,
                            g.SubGrupo.SubGrupoId.ToString(), g.SubGrupo.Nome)).ToList(),
                    u.IsAtivo,
                    u.IsSuperUsuario,
                    u.DominiosBloqueados.Select(d => new UsuarioDetalhesDTO.Dominio(d.ToString(), dominios[d].Nome)).ToList(),
                    u.DominiosAdministrados.Select(d => new UsuarioDetalhesDTO.Dominio(d.ToString(), dominios[d].Nome)).ToList(),
                    u.Email,
                    u.NomeUsuario,
                    u.IsConvitePendente,
                    u.AuditInfo.ToDTO()
                )).ToList();
        }, consistencyToken);
    }

    public async Task<UsuarioLogadoDTO?> GetUsuarioLogadoById(string usuarioId)
    {
        var usuarioCollection = GetCollection<Usuario>(Constants.CollectionNames.USUARIOS, ReadPref.Primary);

        var id = ObjectId.Parse(usuarioId);
        var usuario = await usuarioCollection.FindAsync(u => u.Id == id).FirstOrDefaultAsync();
        if (usuario == null)
            return null;
        return await GetUsuarioLogado(usuario);
    }

    public async Task<UsuarioLogadoDTO?> TestUsuarioCredentials(string? usernameOrEmail, string? password)
    {
        var usuarioCollection = GetCollection<Usuario>(Constants.CollectionNames.USUARIOS, ReadPref.Primary);

        if (usernameOrEmail == null || password == null || usernameOrEmail.IsEmpty() || password.IsEmpty())
            return null;
        usernameOrEmail = usernameOrEmail.Trim().ToLowerInvariant();

        var usuario = await usuarioCollection.FindAsync(u => u.Email == usernameOrEmail || u.NomeUsuario == usernameOrEmail).FirstOrDefaultAsync();
        if (usuario == null)
            return null;
        if (!usuario.TestarSenha(password))
            return null;
        return await GetUsuarioLogado(usuario);
    }

    private async Task<UsuarioLogadoDTO> GetUsuarioLogado(Usuario usuario)
    {
        var gruposCollection = GetCollection<Grupo>(Constants.CollectionNames.GRUPOS, ReadPref.Primary);
        var dominiosCollection = GetCollection<Dominio>(Constants.CollectionNames.DOMINIOS, ReadPref.Primary);
        var estabelecimentosCollection = GetCollection<Estabelecimento>(Constants.CollectionNames.ESTABELECIMENTOS, ReadPref.Primary);

        var allIds = new ObjectId[] { usuario.Id };
        var grupos = await GruposCollection.FindAsync(Filters.Grupos.Create(f =>
            f.And(
                f.Eq(g => g.AuditInfo.RemovidoEm, null),
                f.ElemMatch(g => g.SubGrupos, Builders<SubGrupo>.Filter.AnyIn(sg => sg.UsuarioIds, allIds))))).ToListAsync();
        var allDominioIds = usuario.DominiosAdministrados.Union(usuario.DominiosBloqueados)
            .Union(grupos.Select(g => g.DominioId)).ToList();
        var dominios = (await DominiosCollection.FindAsync(d => allDominioIds.Contains(d.Id)).ToListAsync()).MapByUnique(d => d.Id);

        var dominiosBloqueados = new HashSet<ObjectId>(usuario.DominiosBloqueados);
        var redesIds = grupos
                .SelectMany(g => g.SubGrupos)
                .Where(sg => sg.UsuarioIds.Contains(usuario.Id))
                .SelectMany(sg => sg.PermissoesEstabelecimentos)
                .Where(pe => pe.Seletor.RedeEstabelecimentoId.HasValue)
                .Select(pe => pe.Seletor.RedeEstabelecimentoId!.Value)
                .Distinct()
                .ToList();
        var estabelecimentosIds = grupos
                .SelectMany(g => g.SubGrupos)
                .Where(sg => sg.UsuarioIds.Contains(usuario.Id))
                .SelectMany(sg => sg.PermissoesEstabelecimentos)
                .Where(pe => pe.Seletor.EstabelecimentoId.HasValue)
                .Select(pe => pe.Seletor.EstabelecimentoId!.Value)
                .Distinct()
                .ToList();
        var allEstabelecimentos = await estabelecimentosCollection.FindAsync(e => estabelecimentosIds.Contains(e.Id) || e.Redes.Any(rid => redesIds.Contains(rid))).ToListAsync();
        var subgrupos = new HashSet<ObjectId>(grupos.SelectMany(g => g.SubGrupos).Where(sg => sg.UsuarioIds.Contains(usuario.Id)).Select(sg => sg.SubGrupoId));

        var permissions = allDominioIds.Where(d => !dominiosBloqueados.Contains(d) && dominios[d].IsAtivo).Select(d => new
        {
            DominioId = d,
            IsAdministrador = usuario.DominiosAdministrados.Contains(d),
            DominioName = dominios[d].Nome,
            PermissoesGerais = grupos
                .Where(g => g.DominioId == d)
                .SelectMany(g => g.SubGrupos)
                .Where(sg => subgrupos.Contains(sg.SubGrupoId))
                .SelectMany(sg => sg.PermissoesDominio).Distinct().ToList(),
            PermissoesEstabelecimentos = grupos
                .Where(g => g.DominioId == d)
                .SelectMany(g => g.SubGrupos)
                .Where(sg => subgrupos.Contains(sg.SubGrupoId))
                .SelectMany(sg => sg.PermissoesEstabelecimentos)
                .Select(sg => sg.Seletor.EstabelecimentoId != null ?
                    (Estabelecimentos: new[] { allEstabelecimentos.First(e => e.Id == sg.Seletor.EstabelecimentoId) }, Permissoes: sg) :
                    (Estabelecimentos: allEstabelecimentos.Where(e => e.Redes.Contains(sg.Seletor.RedeEstabelecimentoId!.Value)).ToArray(), Permissoes: sg))
                .SelectMany(sg => sg.Estabelecimentos.Select(e => (Estabelecimento: e, Permissoes: sg.Permissoes)))
                .Where(sg => sg.Estabelecimento.IsAtivo)
                .GroupBy(sg => sg.Estabelecimento.Id)
                .Select(sg => new
                {
                    sg.First().Estabelecimento,
                    Permissoes = sg.SelectMany(sg2 => sg2.Permissoes.Permissoes).Distinct().ToList()
                })
                .ToList()
        }).ToList();

        var isAtivo = usuario.IsAtivo;
        if (isAtivo)
        {
            isAtivo = usuario.DominiosAdministrados.Any() || permissions.Any(p => p.PermissoesGerais.Any() || p.PermissoesEstabelecimentos.Any(pe => pe.Permissoes.Any()));
        }
        if (usuario.IsSuperUsuario)
            isAtivo = true;
        if (usuario.IsConvitePendente)
            isAtivo = false;

        var result = new UsuarioLogadoDTO(usuario.Id.ToString(), usuario.PrimeiroNome, usuario.UltimoNome, usuario.NomeUsuario, usuario.Email, usuario.NomeUsuario, isAtivo, usuario.IsSuperUsuario, usuario.AvatarUrl,
            permissions.Select(p => new UsuarioLogadoDTO.DominioDTO(p.DominioId.ToString(), p.DominioName, p.IsAdministrador, p.IsAdministrador || p.PermissoesGerais.Any(),
                p.PermissoesGerais,
                p.PermissoesEstabelecimentos.Where(e => e.Permissoes.Any()).Select(e =>
                    new UsuarioLogadoDTO.EstabelecimentoDTO(e.Estabelecimento.Id.ToString(), e.Estabelecimento.Nome, e.Permissoes)).ToList())).ToList());

        return result;
    }

}
