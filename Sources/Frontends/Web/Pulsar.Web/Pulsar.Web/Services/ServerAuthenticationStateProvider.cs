using System.Net.Http.Json;
using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace Pulsar.Web.Services;

public class ServerAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly ILogger<ServerAuthenticationStateProvider> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ServerAuthenticationStateProvider(
        IHttpContextAccessor httpContextAccessor,
        ILogger<ServerAuthenticationStateProvider> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var user = GetUser();
        var state = new AuthenticationState(user);

        return Task.FromResult(state);
    }

    private ClaimsPrincipal GetUser(bool useCache = true)
    {
        if (_httpContextAccessor.HttpContext != null)
        {
            return _httpContextAccessor.HttpContext.User;
        }
        else
        {
            return new ClaimsPrincipal(new ClaimsIdentity());
        }
    }
}
