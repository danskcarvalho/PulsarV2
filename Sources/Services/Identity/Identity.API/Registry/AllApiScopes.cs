using Duende.IdentityServer.Models;

namespace Pulsar.Services.Identity.API.Registry;

public static class AllApiScopes
{
    public readonly static ApiScope[] Resources = new ApiScope[]
    {
        //Identity API
        new ApiScope(name: "identity.*"),
        //Convites
        new ApiScope(name: "identity.convites.*"),
        new ApiScope(name: "identity.convites.criar"),
        //Usuarios
        new ApiScope(name: "identity.usuarios.*"),
        new ApiScope(name: "identity.usuarios.listar"),
        new ApiScope(name: "identity.usuarios.logado"),
        new ApiScope(name: "identity.usuarios.dados_basicos"),
        new ApiScope(name: "identity.usuarios.detalhes"),
    };
}
