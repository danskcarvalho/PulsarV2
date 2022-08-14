using Duende.IdentityServer.Models;

namespace Pulsar.Services.Identity.API.Registry;

public static class AllApiScopes
{
    public readonly static ApiScope[] Resources = new ApiScope[]
    {
        //Identity API
        new ApiScope(name: "identity.read", displayName: "Read identity data."),
        new ApiScope(name: "identity.write", displayName: "Write identity data.")
    };
}
