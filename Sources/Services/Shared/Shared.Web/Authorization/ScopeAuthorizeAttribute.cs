using Microsoft.AspNetCore.Authorization;

namespace Pulsar.Services.Shared.API.Authorization;

public class ScopeAuthorizeAttribute : AuthorizeAttribute
{
    public ScopeAuthorizeAttribute(string scope)
    {
        Policy = "Scope_" + scope;
        Scope = scope;
    }

    public string Scope { get; }
}
