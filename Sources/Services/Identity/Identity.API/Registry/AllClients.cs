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

                AllowedCorsOrigins = configuration.GetSection("IdentityServer:Clients:IdentitySwaggerUI:AllowedCorsOrigins").GetChildren().Select(c => c.Value).ToList(),
                RedirectUris = { configuration.GetOrThrow("IdentityServer:Clients:IdentitySwaggerUI:RedirectUri") },
                PostLogoutRedirectUris = { configuration.GetOrThrow("IdentityServer:Clients:IdentitySwaggerUI:PostLogoutRedirectUri") },
                AllowedScopes = AllApiScopes.Resources.Where(s => s.Name.StartsWith("identity.")).Select(s => s.Name).Union(new string[]
                {
                    "openid", "profile", "usuario_admin", "dominio_logado", "dominio_estabelecimento_logado", "estabelecimento_logado", "dominio_logado_perms", "estabelecimento_perms"
                }).ToList()
            }
        };
    }
}
