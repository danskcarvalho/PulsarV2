using Pulsar.Services.Identity.API.Application.BaseTypes;

namespace Pulsar.Services.Identity.API.Application.Queries;

public class UsuarioQueries : IdentityQueries, IUsuarioQueries
{
    public UsuarioQueries(MongoDbSessionFactory factory) : base(factory)
    {
    }

    public async Task<BasicUserInfoDTO?> GetBasicUserInfo(string usuarioId)
    {
        var id = ObjectId.Parse(usuarioId);
        var usuario = await(await Usuarios.FindAsync(u => u.Id == id)).FirstOrDefaultAsync();
        if (usuario == null)
            return null;
        return new BasicUserInfoDTO(usuario.Id.ToString(), usuario.PrimeiroNome, usuario.UltimoNome, usuario.NomeUsuario, usuario.Email, usuario.NomeUsuario, usuario.Avatar?.PublicUrl, usuario.IsSuperUsuario);
    }

    public async Task<UsuarioLogadoDTO?> GetUsuarioLogadoById(string usuarioId)
    {
        var id = ObjectId.Parse(usuarioId);
        var usuario = await (await Usuarios.FindAsync(u => u.Id == id)).FirstOrDefaultAsync();
        if (usuario == null)
            return null;
        return await GetUsuarioLogado(usuario);
    }

    public async Task<UsuarioLogadoDTO?> TestUsuarioCredentials(string? usernameOrEmail, string? password)
    {

        if (usernameOrEmail == null || password == null || usernameOrEmail.IsEmpty() || password.IsEmpty())
            return null;
        usernameOrEmail = usernameOrEmail.Trim().ToLowerInvariant();

        var usuario = await (await Usuarios.FindAsync(u => u.Email == usernameOrEmail || u.NomeUsuario == usernameOrEmail)).FirstOrDefaultAsync();
        if (usuario == null)
            return null;
        if (!usuario.TestarSenha(password))
            return null;
        return await GetUsuarioLogado(usuario);
    }

    private async Task<UsuarioLogadoDTO> GetUsuarioLogado(Usuario usuario)
    {
        var gruposIds = usuario.Grupos.Select(g => g.GrupoId).ToList();
        var grupos = await (await Grupos.FindAsync(g => gruposIds.Contains(g.Id))).ToListAsync();
        var dominioIds = grupos.Select(g => g.DominioId).Distinct().ToList();
        var dominiosBloqueados = new HashSet<ObjectId>(usuario.DominiosBloqueados);
        var todosDominiosIds = dominioIds.Union(usuario.DominiosAdministrados).Union(usuario.DominiosBloqueados).ToList();
        var dominios = (await (await Dominios.FindAsync(d => todosDominiosIds.Contains(d.Id))).ToListAsync()).MapByUnique(d => d.Id);
        var redesIds = grupos
                .SelectMany(g => g.SubGrupos)
                .Where(sg => usuario.Grupos.Any(ug => ug.SubGrupoId == sg.Id))
                .SelectMany(sg => sg.PermissoesEstabelecimentos)
                .Where(pe => pe.Seletor.RedeEstabelecimentoId.HasValue)
                .Select(pe => pe.Seletor.RedeEstabelecimentoId!.Value)
                .Distinct()
                .ToList();
        var estabelecimentosIds = grupos
                .SelectMany(g => g.SubGrupos)
                .Where(sg => usuario.Grupos.Any(ug => ug.SubGrupoId == sg.Id))
                .SelectMany(sg => sg.PermissoesEstabelecimentos)
                .Where(pe => pe.Seletor.EstabelecimentoId.HasValue)
                .Select(pe => pe.Seletor.EstabelecimentoId!.Value)
                .Distinct()
                .ToList();
        var allEstabelecimentos = await (await Estabelecimentos.FindAsync(e => estabelecimentosIds.Contains(e.Id) ||
            e.Redes.Any(rid => redesIds.Contains(rid)))).ToListAsync();
        var subgrupos = new HashSet<ObjectId>(usuario.Grupos.Select(ug => ug.SubGrupoId));

        var permissions = dominioIds.Union(usuario.DominiosAdministrados).Where(d => !dominiosBloqueados.Contains(d) && dominios[d].IsAtivo).Select(d => new
        {
            DominioId = d,
            IsAdministrador = usuario.DominiosAdministrados.Contains(d),
            DominioName = dominios[d].Nome,
            PermissoesGerais = grupos
                .Where(g => g.DominioId == d)
                .SelectMany(g => g.SubGrupos)
                .Where(sg => subgrupos.Contains(sg.Id))
                .SelectMany(sg => sg.PermissoesDominio).Distinct().ToList(),
            PermissoesEstabelecimentos = grupos
                .Where(g => g.DominioId == d)
                .SelectMany(g => g.SubGrupos)
                .Where(sg => subgrupos.Contains(sg.Id))
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

        var result = new UsuarioLogadoDTO(usuario.Id.ToString(), usuario.PrimeiroNome, usuario.UltimoNome, usuario.NomeUsuario, usuario.Email, usuario.NomeUsuario, isAtivo, usuario.IsSuperUsuario, usuario.Avatar?.PublicUrl,
            permissions.Select(p => new UsuarioLogadoDTO.DominioDTO(p.DominioId.ToString(), p.DominioName, p.IsAdministrador, p.IsAdministrador || p.PermissoesGerais.Any(),
                p.PermissoesGerais,
                p.PermissoesEstabelecimentos.Where(e => e.Permissoes.Any()).Select(e =>
                    new UsuarioLogadoDTO.EstabelecimentoDTO(e.Estabelecimento.Id.ToString(), e.Estabelecimento.Nome, e.Permissoes)).ToList())).ToList());

        return result;
    }
}
