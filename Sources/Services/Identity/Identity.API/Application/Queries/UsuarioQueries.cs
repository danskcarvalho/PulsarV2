using Pulsar.Services.Identity.API.Application.BaseTypes;
using Pulsar.Services.Identity.Domain.Aggregates.Usuarios;
using Pulsar.Services.Shared.DTOs;

namespace Pulsar.Services.Identity.API.Application.Queries;

public partial class UsuarioQueries : IdentityQueries, IUsuarioQueries
{
    public UsuarioQueries(IdentityQueriesContext ctx) : base(ctx)
    {
    }

    public async Task<PaginatedListDTO<UsuarioListadoDTO>> FindUsuarios(UsuarioFiltroDTO filtro)
    {
        return await this.StartCausallyConsistentSectionAsync(async ct =>
        {
            if (filtro.Cursor is null)
                return await FindUsuariosWithoutCursor(filtro.Filtro, filtro.Limit ?? 50);
            else
                return await FindUsuariosByCursor(filtro.Cursor, filtro.Limit ?? 50);
        }, filtro.ConsistencyToken);
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
            var allDominioIds = usuarios.SelectMany(u => u.DominiosAdministrados).Union(usuarios.SelectMany(u => u.DominiosBloqueados))
                .Union(usuarios.SelectMany(u => u.Grupos.Select(g => g.DominioId))).ToList();
            var allGruposIds = usuarios.SelectMany(u => u.Grupos.Select(g => g.GrupoId)).ToList();
            var dominios = (await DominiosCollection.FindAsync(d => allDominioIds.Contains(d.Id)).ToListAsync()).MapByUnique(d => d.Id);
            var grupos = await GruposCollection.FindAsync(g => allGruposIds.Contains(g.Id) && g.AuditInfo.RemovidoEm == null).ToListAsync();
            var subgruposPorIds = grupos.SelectMany(g => g.SubGrupos.Select(sg => (Grupo: g, SubGrupo: sg))).MapByUnique(x => (GrupoId: x.Grupo.Id, SubGrupoId: x.SubGrupo.SubGrupoId));

            return usuarios.Select(u => new UsuarioDetalhesDTO(
                    u.Id.ToString(),
                    u.AvatarUrl,
                    u.PrimeiroNome,
                    u.UltimoNome,
                    u.NomeCompleto,
                    u.Grupos
                        .Where(g => subgruposPorIds.ContainsKey((g.GrupoId, g.SubGrupoId)))
                        .Select(g => new UsuarioDetalhesDTO.UsuarioGrupo(
                            g.DominioId.ToString(), dominios[g.DominioId].Nome,
                            g.GrupoId.ToString(), subgruposPorIds[(g.GrupoId, g.SubGrupoId)].Grupo.Nome,
                            g.SubGrupoId.ToString(), subgruposPorIds[(g.GrupoId, g.SubGrupoId)].SubGrupo.Nome)).ToList(),
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

        var gruposIds = usuario.Grupos.Select(g => g.GrupoId).ToList();
        var grupos = await gruposCollection.FindAsync(g => gruposIds.Contains(g.Id) && g.AuditInfo.RemovidoEm == null).ToListAsync();
        var dominioIds = grupos.Select(g => g.DominioId).Distinct().ToList();
        var dominiosBloqueados = new HashSet<ObjectId>(usuario.DominiosBloqueados);
        var todosDominiosIds = dominioIds.Union(usuario.DominiosAdministrados).Union(usuario.DominiosBloqueados).ToList();
        var dominios = (await dominiosCollection.FindAsync(d => todosDominiosIds.Contains(d.Id)).ToListAsync()).MapByUnique(d => d.Id);
        var redesIds = grupos
                .SelectMany(g => g.SubGrupos)
                .Where(sg => usuario.Grupos.Any(ug => ug.SubGrupoId == sg.SubGrupoId))
                .SelectMany(sg => sg.PermissoesEstabelecimentos)
                .Where(pe => pe.Seletor.RedeEstabelecimentoId.HasValue)
                .Select(pe => pe.Seletor.RedeEstabelecimentoId!.Value)
                .Distinct()
                .ToList();
        var estabelecimentosIds = grupos
                .SelectMany(g => g.SubGrupos)
                .Where(sg => usuario.Grupos.Any(ug => ug.SubGrupoId == sg.SubGrupoId))
                .SelectMany(sg => sg.PermissoesEstabelecimentos)
                .Where(pe => pe.Seletor.EstabelecimentoId.HasValue)
                .Select(pe => pe.Seletor.EstabelecimentoId!.Value)
                .Distinct()
                .ToList();
        var allEstabelecimentos = await estabelecimentosCollection.FindAsync(e => estabelecimentosIds.Contains(e.Id) || e.Redes.Any(rid => redesIds.Contains(rid))).ToListAsync();
        var subgrupos = new HashSet<ObjectId>(usuario.Grupos.Select(ug => ug.SubGrupoId));

        var permissions = dominioIds.Union(usuario.DominiosAdministrados).Where(d => !dominiosBloqueados.Contains(d) && dominios[d].IsAtivo).Select(d => new
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
                .SelectMany(sg => sg.Estabelecimentos.Select(e => (Estabelecimento: e, Permissoes: sg.Item2)))
                .Where(sg => sg.Estabelecimento.IsAtivo)
                .GroupBy(sg => sg.Estabelecimento.Id)
                .Select(sg => new
                {
                    Estabelecimento = sg.First().Estabelecimento,
                    Permissoes = sg.SelectMany(sg2 => sg2.Permissoes.Permissoes).Distinct().ToList()
                }).ToList()
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
