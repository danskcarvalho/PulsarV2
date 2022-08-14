using Duende.IdentityServer.Models;

namespace Pulsar.Services.Identity.API.Registry;

public static class AllClients
{
    public static Client[] Resources(IConfiguration configuration)
    {
        return new Client[]
        {
            new Client
            {
                ClientId = "identityswaggerui",
                ClientName = "Identity Swagger UI",
                AllowedGrantTypes = GrantTypes.Code,
                RequireClientSecret = false,
                AllowAccessTokensViaBrowser = true,

                RedirectUris = { configuration["IdentityServer:Clients:IdentitySwaggerUI:RedirectUri"] },
                PostLogoutRedirectUris = { configuration["IdentityServer:Clients:IdentitySwaggerUI:PostLogoutRedirectUri"] },
                AllowedScopes =
                {
                    "identity.read", "identity.write", "openid", "profile", "usuario_admin", "dominio_logado", "dominio_estabelecimento_logado", "estabelecimento_logado", "dominio_logado_perms", "estabelecimento_perms"
                }
            },
        };
    }
}
