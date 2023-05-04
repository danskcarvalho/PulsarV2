using Pulsar.Services.Identity.Contracts.DTOs;
using System.Globalization;
using System.Security.Claims;

namespace Identity.UnitTests.BaseTypes;

public partial class IdentityScenarios
{
    public class UsersRegistry
    {
        public ClaimsPrincipal Admin
        {
            get
            {
                var usuarioLogado = new UsuarioLogadoDTO(IdentityDatabase.AdminUserId, "Administrador", null, "Administrador", null, "administrador", true, true, null, new List<UsuarioLogadoDTO.DominioDTO>());
                var login = new LoginDTO()
                {
                    UsernameOrEmail = "administrador"
                };
                return BuildClaimsPrincipal(usuarioLogado, login);
            }
        }

        public ClaimsPrincipal Samantha
        {
            get
            {
                var usuarioLogado = new UsuarioLogadoDTO(IdentityDatabase.SamanthaUserId, "Samantha", null, "Samantha", "samantha.test@gmail.com", "samantha", true, false, null, 
                    new List<UsuarioLogadoDTO.DominioDTO>()
                    {
                        new UsuarioLogadoDTO.DominioDTO(IdentityDatabase.DominioPadraoId, "PADRÃO", true, true, Enum.GetValues<PermissoesDominio>().ToList(), new List<UsuarioLogadoDTO.EstabelecimentoDTO>())
                    });
                var login = new LoginDTO()
                {
                    UsernameOrEmail = "samantha",
                    DominioId = IdentityDatabase.DominioPadraoId
                };
                return BuildClaimsPrincipal(usuarioLogado, login);
            }
        }

        public ClaimsPrincipal Alexia
        {
            get
            {
                var usuarioLogado = new UsuarioLogadoDTO(IdentityDatabase.AlexiaUserId, "Alexia", null, "Alexia", "alexia.test@gmail.com", "alexia", true, false, null,
                    new List<UsuarioLogadoDTO.DominioDTO>()
                    {
                        new UsuarioLogadoDTO.DominioDTO(IdentityDatabase.DominioPadraoId, "PADRÃO", false, false, new List<PermissoesDominio>(), new List<UsuarioLogadoDTO.EstabelecimentoDTO>()
                        {
                            new UsuarioLogadoDTO.EstabelecimentoDTO(IdentityDatabase.EstabelecimentoPadraoId, "PADRÃO", Enum.GetValues<PermissoesEstabelecimento>().ToList())
                        })
                    });
                var login = new LoginDTO()
                {
                    UsernameOrEmail = "samantha",
                    EstabelecimentoId = IdentityDatabase.EstabelecimentoPadraoId,
                    DominioId = IdentityDatabase.DominioPadraoId
                };
                return BuildClaimsPrincipal(usuarioLogado, login);
            }
        }

        private ClaimsPrincipal BuildClaimsPrincipal(UsuarioLogadoDTO userInfo, LoginDTO login)
        {
            var claims = new List<Claim> {
                new Claim("sub", GetSub(userInfo, login)),
                new Claim("email", userInfo.Email ?? string.Empty),
                new Claim("username", userInfo.NomeUsuario),
                new Claim("uag", userInfo.IsSuperUsuario ? "true" : "false"),
                new Claim("uad", login.EstabelecimentoId == null && userInfo.Dominios.FirstOrDefault(d => d.Id == login.DominioId)?.IsAdministrador == true ? "true" : "false"),
                new Claim("d", login.DominioId != null && login.EstabelecimentoId == null ? login.DominioId : string.Empty),
                new Claim("de", login.DominioId != null && login.EstabelecimentoId != null ? login.DominioId : string.Empty),
                new Claim("e", login.EstabelecimentoId != null ? login.EstabelecimentoId : string.Empty),
                new Claim("dp", GetDominioPerms(userInfo, login)),
                new Claim("ep", GetEstabelecimentoPerms(userInfo, login)),
            };

            var identity = new ClaimsIdentity(claims, "pwd");
            var user = new ClaimsPrincipal(identity);
            return user;

            string GetEstabelecimentoPerms(UsuarioLogadoDTO userInfo, LoginDTO login)
            {
                if (login.DominioId != null && login.EstabelecimentoId != null)
                    return String.Join(',', userInfo.Dominios.First(d => d.Id == login.DominioId).Estabelecimentos.First(e => e.Id == login.EstabelecimentoId).Permissoes.Select(p => (int)p).OrderBy(p => p).Select(p => p.ToString(CultureInfo.InvariantCulture)));
                else
                    return String.Empty;
            }

            string GetDominioPerms(UsuarioLogadoDTO userInfo, LoginDTO login)
            {
                if (login.DominioId != null && login.EstabelecimentoId == null)
                    return String.Join(',', userInfo.Dominios.First(d => d.Id == login.DominioId).Permissoes.Select(p => (int)p).OrderBy(p => p).Select(p => p.ToString(CultureInfo.InvariantCulture)));
                else
                    return String.Empty;
            }

            string GetSub(UsuarioLogadoDTO userInfo, LoginDTO login)
            {
                return $"{login.DominioId ?? "_"}/{login.EstabelecimentoId ?? "_"}/{userInfo.Id}";
            }
        }
    }
}
