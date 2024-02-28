using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pulsar.Services.Identity.Contracts.DTOs;
using Pulsar.Services.Identity.Contracts.Enumerations;
using Pulsar.Services.Identity.Domain.Aggregates.Usuarios;
using Pulsar.Services.Identity.UI.Pages;
using System.Globalization;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace Identity.IntegrationTests.BaseTypes;


public class MockedAuthOptions : AuthenticationSchemeOptions
{
    public MockedAuthOptions(string usuarioId)
    {
        UsuarioId = usuarioId;
    }

    public MockedAuthOptions(string dominioId, string usuarioId)
    {
        UsuarioId = usuarioId;
        DominioId = dominioId;
    }

    public MockedAuthOptions(string dominioId, string estabelecimentoId, string usuarioId)
    {
        UsuarioId = usuarioId;
        DominioId = dominioId;
        EstabelecimentoId = estabelecimentoId;
    }

    public MockedAuthOptions()
    {
    }

    public string? DominioId { get; set; }
    public string? EstabelecimentoId { get; set; }
    public string? UsuarioId { get; set; }
}

public class MockedAuthHandler : AuthenticationHandler<MockedAuthOptions>
{
    private const string API_SCOPE = "identity.*";

    public MockedAuthHandler(
        IOptionsMonitor<MockedAuthOptions> options,
        ILoggerFactory logger, UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        return Task.FromResult(GetAuthenticateResult(Options.UsuarioId ?? throw new InvalidOperationException("it's necessary to provide a userId"), Options.DominioId, Options.EstabelecimentoId));
    }

    AuthenticateResult GetAuthenticateResult(string usuarioId, string? dominioId, string? estabelecimentoId)
    {
        var claims = new List<Claim> {
                new Claim("sub", GetSub(usuarioId, dominioId, estabelecimentoId)),
                new Claim("email", string.Empty),
                new Claim("username", $"user_{usuarioId}"),
                new Claim("uag", usuarioId == Usuario.SuperUsuarioId.ToString().ToLowerInvariant() ? "true" : "false"),
                new Claim("uad", estabelecimentoId == null && dominioId != null ? "true" : "false"),
                new Claim("d", dominioId != null && estabelecimentoId == null ? dominioId : string.Empty),
                new Claim("de", dominioId != null && estabelecimentoId != null ? dominioId : string.Empty),
                new Claim("e", estabelecimentoId != null ? estabelecimentoId : string.Empty),
                new Claim("dp", GetDominioPerms(dominioId, estabelecimentoId)),
                new Claim("ep", GetEstabelecimentoPerms(dominioId, estabelecimentoId)),
                new Claim("scope", API_SCOPE)
            };

        var identity = new ClaimsIdentity(claims, "pwd");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Testing");

        return AuthenticateResult.Success(ticket);

        string GetEstabelecimentoPerms(string? dominioId, string? estabelecimentoId)
        {
            if (estabelecimentoId != null)
                return String.Join(',', Enum.GetValues<PermissoesEstabelecimento>().Select(p => (int)p).OrderBy(p => p).Select(p => p.ToString(CultureInfo.InvariantCulture)));
            else
                return string.Empty;
        }

        string GetDominioPerms(string? dominioId, string? estabelecimentoId)
        {
            if (dominioId != null && estabelecimentoId == null)
                return String.Join(',', Enum.GetValues<PermissoesDominio>().Select(p => (int)p).OrderBy(p => p).Select(p => p.ToString(CultureInfo.InvariantCulture)));
            else
                return string.Empty;
        }

        string GetSub(string usuarioId, string? dominioId, string? estabelecimentoId)
        {
            return $"{dominioId ?? "_"}/{estabelecimentoId ?? "_"}/{usuarioId}";
        }
    }
}
