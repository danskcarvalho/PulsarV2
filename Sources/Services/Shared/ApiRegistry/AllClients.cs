using Duende.IdentityServer.Models;
using Microsoft.Extensions.Configuration;

namespace Pulsar.Services.ApiRegistry;

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
                //You can request a refresh token by adding a scope called offline_access to the scope parameter list of the authorize request.
                //The clients needs to be explicitly authorized to be able to use refresh tokens by setting the AllowOfflineAccess property to true.
                //AllowOfflineAccess = true,

                AllowedCorsOrigins = configuration.GetSection("IdentityServer:Clients:IdentitySwaggerUI:AllowedCorsOrigins").GetChildren().Select(c => c.Value).Where(c => c is not null).ToList()!,
                RedirectUris = { configuration.GetOrThrow("IdentityServer:Clients:IdentitySwaggerUI:RedirectUri") },
                PostLogoutRedirectUris = { configuration.GetOrThrow("IdentityServer:Clients:IdentitySwaggerUI:PostLogoutRedirectUri") },
                AllowedScopes = AllApiScopes.Resources.Where(s => s.Name.StartsWith("identity.")).Select(s => s.Name).Union(
                [
                    "openid", "profile", "usuario_admin", "dominio_logado", "dominio_estabelecimento_logado", "estabelecimento_logado", "dominio_logado_perms", "estabelecimento_perms"
                ]).ToList()
            },
            new Client
            {
                ClientId = "catalogswaggerui",
                ClientName = "Catalog Swagger UI",
                AllowedGrantTypes = GrantTypes.Code,
                RequireClientSecret = false,
                AllowAccessTokensViaBrowser = true,
                //You can request a refresh token by adding a scope called offline_access to the scope parameter list of the authorize request.
                //The clients needs to be explicitly authorized to be able to use refresh tokens by setting the AllowOfflineAccess property to true.
                //AllowOfflineAccess = true,

                AllowedCorsOrigins = configuration.GetSection("IdentityServer:Clients:CatalogSwaggerUI:AllowedCorsOrigins").GetChildren().Select(c => c.Value).Where(c => c is not null).ToList()!,
                RedirectUris = { configuration.GetOrThrow("IdentityServer:Clients:CatalogSwaggerUI:RedirectUri") },
                PostLogoutRedirectUris = { configuration.GetOrThrow("IdentityServer:Clients:CatalogSwaggerUI:PostLogoutRedirectUri") },
                AllowedScopes = AllApiScopes.Resources.Where(s => s.Name.StartsWith("catalog.")).Select(s => s.Name).Union(
                [
                    "openid", "profile", "usuario_admin", "dominio_logado", "dominio_estabelecimento_logado", "estabelecimento_logado", "dominio_logado_perms", "estabelecimento_perms"
                ]).ToList()
            }
        };
    }
}
