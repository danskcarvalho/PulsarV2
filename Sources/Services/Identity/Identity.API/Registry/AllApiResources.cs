using Duende.IdentityServer.Models;

namespace Pulsar.Services.Identity.API.Registry;

public static class AllApiResources
{
    public readonly static ApiResource[] Resources = new ApiResource[]
    {
        new ApiResource("identity", "Identity API")
        {
            Scopes = AllApiScopes.Resources.Where(s => s.Name.StartsWith("identity.")).Select(s => s.Name).ToList(),
            UserClaims =
            {
               "d", "de", "e", "dp", "ep", "uag", "uad"
            }
        },
    };
}
