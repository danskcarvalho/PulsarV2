using Microsoft.AspNetCore.Authorization;

namespace Pulsar.Services.Identity.API.Authorization;

public class ScopeAuthorizeAttribute : AuthorizeAttribute
{
    public ScopeAuthorizeAttribute(string scope)
    {
        Policy = "Scope_identity." + scope;
        Scope = scope;
    }

    public string Scope { get; }
}
