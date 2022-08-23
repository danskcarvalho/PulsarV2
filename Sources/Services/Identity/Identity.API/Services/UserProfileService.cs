using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Pulsar.Services.Identity.Domain.Aggregates.Usuarios;
using Pulsar.Services.Identity.UI.Pages;
using System.Globalization;
using System.Security.Claims;

namespace Pulsar.Services.Identity.API.Services;

public class UserProfileService : IProfileService
{
    private readonly IUsuarioQueries _usuarioQueries;

    public UserProfileService(IUsuarioQueries usuarioQueries)
    {
        _usuarioQueries = usuarioQueries;
    }

    // this method adds claims that should go into the token to context.IssuedClaims
    public virtual async Task GetProfileDataAsync(ProfileDataRequestContext context)
    {
        var requestedClaimsTypes = context.RequestedClaimTypes;
        var user = context.Subject;

        // your implementation to retrieve the requested information
        var claims = await GetRequestedClaims(user, requestedClaimsTypes);
        context.IssuedClaims.AddRange(claims);
    }

    private async Task<List<Claim>> GetRequestedClaims(ClaimsPrincipal user, IEnumerable<string> requestedClaimsTypes)
    {
        var requestedNotPresent = requestedClaimsTypes.Where(c => !user.Claims.Any(uc => c == uc.Type)).ToList();
        var allProfileClaims = new HashSet<string>(AllIdentityResources.Resources.First(r => r.Name == "profile").UserClaims);
        var allNonProfileClaims = new HashSet<string>(AllIdentityResources.Resources.Where(r => r.Name != "profile" && r.Name != IdentityServerConstants.StandardScopes.OpenId).SelectMany(r => r.UserClaims));
        var profileClaims = requestedNotPresent.Where(c => allProfileClaims.Contains(c)).ToList();
        var nonProfileClaims = requestedNotPresent.Where(c => allNonProfileClaims.Contains(c)).ToList();
        var usuarioId = GetId(user);
        if (usuarioId == null)
            return new List<Claim>();
        var dominioId = GetDominioId(user);
        var estabelecimentoId = GetEstabelecimentoId(user);

        BasicUserInfoDTO? basicInfo = null;
        UsuarioLogadoDTO? userInfo = null;

        if (nonProfileClaims.Count != 0)
        {
            userInfo = await _usuarioQueries.GetUsuarioLogadoById(usuarioId);
            if (userInfo == null)
                return new List<Claim>();
            basicInfo = new BasicUserInfoDTO(userInfo.Id, userInfo.PrimeiroNome, userInfo.UltimoNome, userInfo.NomeCompleto, userInfo.Email, userInfo.NomeUsuario, userInfo.AvatarUrl, userInfo.IsSuperUsuario);
        }
        else if (profileClaims.Count != 0)
        {
            basicInfo = await _usuarioQueries.GetBasicUserInfo(usuarioId, null);
            if (basicInfo == null)
                return new List<Claim>();
        }

        var claims = new List<Claim>();
        foreach (var c in requestedClaimsTypes.Where(c => allProfileClaims.Contains(c)))
        {
            var existing = user.Claims.FirstOrDefault(uc => c == uc.Type);
            if (existing != null)
            {
                claims.Add(existing);
                continue;
            }
            if (c == "first_name")
                claims.Add(new Claim("first_name", basicInfo!.PrimeiroNome));
            else if (c == "last_name")
                claims.Add(new Claim("last_name", basicInfo!.UltimoNome ?? string.Empty));
            else if (c == "name")
                claims.Add(new Claim("name", basicInfo!.NomeCompleto));
            else if (c == "avatar_url")
                claims.Add(new Claim("avatar_url", basicInfo!.AvatarUrl ?? string.Empty));
            else if (c == "email")
                claims.Add(new Claim("email", basicInfo!.Email ?? String.Empty));
            else if (c == "username")
                claims.Add(new Claim("username", basicInfo!.NomeUsuario));
        }

        foreach (var c in requestedClaimsTypes.Where(c => allNonProfileClaims.Contains(c)))
        {
            var existing = user.Claims.FirstOrDefault(uc => c == uc.Type);
            if (existing != null)
            {
                claims.Add(existing);
                continue;
            }

            if (c == "uag")
                claims.Add(new Claim("uag", userInfo!.IsSuperUsuario ? "true" : "false"));
            else if (c == "uad")
                claims.Add(new Claim("uad", estabelecimentoId == null && userInfo!.Dominios.FirstOrDefault(d => d.Id == dominioId)?.IsAdministrador == true ? "true" : "false"));
            else if (c == "d")
                claims.Add(new Claim("d", dominioId != null && estabelecimentoId == null ? dominioId : string.Empty));
            else if (c == "dn")
                claims.Add(new Claim("dn", dominioId != null && estabelecimentoId == null ? userInfo!.Dominios.First(d => d.Id == dominioId).Nome : string.Empty));
            else if (c == "de")
                claims.Add(new Claim("de", dominioId != null && estabelecimentoId != null ? dominioId : string.Empty));
            else if (c == "den")
                claims.Add(new Claim("den", dominioId != null && estabelecimentoId != null ? userInfo!.Dominios.First(d => d.Id == dominioId).Nome : string.Empty));
            else if (c == "e")
                claims.Add(new Claim("e", estabelecimentoId != null ? estabelecimentoId : string.Empty));
            else if (c == "en")
                claims.Add(new Claim("en", estabelecimentoId != null ? userInfo!.Dominios.First(d => d.Id == dominioId).Estabelecimentos.First(e => e.Id == estabelecimentoId).Nome : string.Empty));
            else if (c == "dp")
                claims.Add(new Claim("dp", GetDominioPerms(userInfo!, dominioId, estabelecimentoId)));
            else if (c == "ep")
                claims.Add(new Claim("ep", GetEstabelecimentoPerms(userInfo!, dominioId, estabelecimentoId)));
        }

        return claims;
    }

    private string GetEstabelecimentoPerms(UsuarioLogadoDTO userInfo, string? dominioId, string? estabelecimentoId)
    {
        if (dominioId != null && estabelecimentoId != null)
            return String.Join(',', userInfo.Dominios.First(d => d.Id == dominioId).Estabelecimentos.First(e => e.Id == estabelecimentoId).Permissoes.Select(p => (int)p).OrderBy(p => p).Select(p => p.ToString(CultureInfo.InvariantCulture)));
        else
            return String.Empty;
    }

    private string GetDominioPerms(UsuarioLogadoDTO userInfo, string? dominioId, string? estabelecimentoId)
    {
        if (dominioId != null && estabelecimentoId == null)
            return String.Join(',', userInfo.Dominios.First(d => d.Id == dominioId).Permissoes.Select(p => (int)p).OrderBy(p => p).Select(p => p.ToString(CultureInfo.InvariantCulture)));
        else
            return String.Empty;
    }

    private string? GetEstabelecimentoId(ClaimsPrincipal user)
    {
        var subjectId = user.Claims.Where(x => x.Type == "sub" || x.Type == ClaimTypes.NameIdentifier).FirstOrDefault()?.Value;
        if (subjectId == null)
            return null;

        var v = subjectId.Split('/', StringSplitOptions.RemoveEmptyEntries)[1];
        return v == "_" ? null : v;
    }

    private string? GetDominioId(ClaimsPrincipal user)
    {
        var subjectId = user.Claims.Where(x => x.Type == "sub" || x.Type == ClaimTypes.NameIdentifier).FirstOrDefault()?.Value;
        if (subjectId == null)
            return null;

        var v = subjectId.Split('/', StringSplitOptions.RemoveEmptyEntries)[0];
        return v == "_" ? null : v;
    }

    private static string? GetId(ClaimsPrincipal user)
    {
        var subjectId = user.Claims.Where(x => x.Type == "sub" || x.Type == ClaimTypes.NameIdentifier).FirstOrDefault()?.Value;
        if (subjectId == null)
            return null;

        return subjectId.Split('/', StringSplitOptions.RemoveEmptyEntries)[2];
    }

    // this method allows to check if the user is still "enabled" per token request
    public virtual async Task IsActiveAsync(IsActiveContext context)
    {
        var authTime = DateTimeOffset.FromUnixTimeSeconds(int.Parse(context.Subject.Claims.First(c => c.Type == "auth_time").Value)).UtcDateTime;
        var passed = (DateTime.UtcNow - authTime).Duration();
        if (passed <= TimeSpan.FromMinutes(30))
        {
            context.IsActive = true;
            return;
        }

        context.IsActive = false;
        var usuarioId = GetId(context.Subject);
        if (usuarioId == null)
            return;

        var dominioId = GetDominioId(context.Subject);
        var estabelecimentoId = GetEstabelecimentoId(context.Subject);

        UsuarioLogadoDTO? userInfo = await _usuarioQueries.GetUsuarioLogadoById(usuarioId);
        if (userInfo == null)
            return;
        if (dominioId != null && estabelecimentoId == null && !userInfo.Dominios.Any(d => d.Id == dominioId && d.Permissoes.Any()))
            return;
        if (dominioId != null && estabelecimentoId != null && !userInfo.Dominios.Any(d => d.Id == dominioId && d.Estabelecimentos.Any(e => e.Id == estabelecimentoId && e.Permissoes.Any())))
            return;

        context.IsActive = userInfo.IsAtivo;
    }
}
