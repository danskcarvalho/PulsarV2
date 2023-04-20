using Duende.IdentityServer.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Configuration;
using System.Globalization;
using System.Security.Claims;

namespace Pulsar.Services.Identity.API.Controllers;

[ApiController]
[Route("v2/login")]
[ApiExplorerSettings(IgnoreApi = true)]
public class LoginController : IdentityController
{
    private readonly IIdentityServerInteractionService _interaction;

    public LoginController(IIdentityServerInteractionService interaction, IdentityControllerContext context) : base(context)
    {
        _interaction = interaction;
    }

    [HttpPost]
    [Route("test")]
    public async Task<ActionResult<UsuarioLogadoDTO>> TestCredentials([FromBody] UsuarioSenhaDTO usuarioSenha)
    {
        var r = await UsuarioQueries.TestUsuarioCredentials(usuarioSenha.UsernameOrEmail, usuarioSenha.Senha);
        if (r == null)
            return NotFound();
        else
            return Ok(r);
    }

    [HttpPost]
    public async Task<ActionResult<LoginResultDTO>> Login([FromBody] LoginDTO login)
    {
        var user = await UsuarioQueries.TestUsuarioCredentials(login.UsernameOrEmail, login.Senha);

        if (user is not null && user.ValidateLogin(login.DominioId, login.EstabelecimentoId))
        {
            var tokenLifetime = Configuration.GetValue("TokenLifetimeDays", 14);

            var props = new AuthenticationProperties
            {
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(tokenLifetime),
                AllowRefresh = true,
                RedirectUri = login.ReturnUrl,
                IsPersistent = true
            };

            // make sure the returnUrl is still valid, and if yes - redirect back to authorize endpoint
            if (_interaction.IsValidReturnUrl(login.ReturnUrl))
            {
                await SignInAsync(user, login, props);
                return Ok(new LoginResultDTO() { Ok = true, RedirectUrl = login.ReturnUrl });
            }
            else
                return Ok(new LoginResultDTO() { Ok = false, Erro = "Url de retorno inválida." });
        }
        else
        {
            if(user is null)
                return Ok(new LoginResultDTO() { Ok = false, Erro = "O usuário ou senha informados não é válido." });
            else
                return Ok(new LoginResultDTO() { Ok = false, Erro = "Não é possível logar no domínio/estabelecimento informado." });

        }

        async Task SignInAsync(UsuarioLogadoDTO userInfo, LoginDTO login, AuthenticationProperties props)
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
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, user, props);

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
