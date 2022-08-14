using Duende.IdentityServer.Models;

namespace Pulsar.Services.Identity.API.Registry;

public static class AllIdentityResources
{
    public readonly static IdentityResource[] Resources = new IdentityResource[]
    {
        new IdentityResources.OpenId(),
        new IdentityResource(
            name: "profile",
            userClaims: new[] { "first_name", "last_name", "name", "avatar_url", "email", "username" },
            displayName: "User profile data"),
        new IdentityResource(
            name: "usuario_admin",
            userClaims: new[] { "uag", "uad" }), /*"usuario_admin_geral", "usuario_admin_dominio"*/
        new IdentityResource(
            name: "dominio_logado",
            userClaims: new[] { "d", "dn" }, /*"dominio_logado_id", "dominio_logado_nome"*/
            displayName: "Domínio logado corrente"),
        new IdentityResource(
            name: "dominio_estabelecimento_logado",
            userClaims: new[] { "de", "den" }, /*"dominio_estabelecimento_logado_id", "dominio_estabelecimento_logado_nome"*/
            displayName: "Domínio do estabelecimento logado"),
         new IdentityResource(
            name: "estabelecimento_logado",
            userClaims: new[] { "e", "en" }, /*"estabelecimento_logado_id", "estabelecimento_logado_nome"*/
            displayName: "Estabelecimento logado"),
          new IdentityResource(
            name: "dominio_logado_perms",
            userClaims: new[] { "dp" }, /*"dominio_logado_perms" */
            displayName: "Permissões do domínio logado"),
          new IdentityResource(
            name: "estabelecimento_logado_perms",
            userClaims: new[] { "ep" }, /*"estabelecimento_logado_perms" */
            displayName: "Permissões do estabelecimento logado")
    };
}
