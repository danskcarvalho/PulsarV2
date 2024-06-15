using Duende.IdentityServer.Models;

namespace Pulsar.Services.ApiRegistry;

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
        new ApiScope(name: "identity.usuarios.bloquear"),
        new ApiScope(name: "identity.usuarios.desbloquear"),
        new ApiScope(name: "identity.usuarios.editar_meus_dados"),
        new ApiScope(name: "identity.usuarios.mudar_minha_senha"),
        new ApiScope(name: "identity.usuarios.mudar_meu_avatar"),
        //Domínios
        new ApiScope(name: "identity.dominios.*"),
        new ApiScope(name: "identity.dominios.listar"),
        new ApiScope(name: "identity.dominios.logado"),
        new ApiScope(name: "identity.dominios.esconder"),
        new ApiScope(name: "identity.dominios.mostrar"),
        new ApiScope(name: "identity.dominios.detalhes"),
        new ApiScope(name: "identity.dominios.usuarios_bloqueados"),
        new ApiScope(name: "identity.dominios.criar"),
        new ApiScope(name: "identity.dominios.editar"),
        new ApiScope(name: "identity.dominios.bloquear"),
        new ApiScope(name: "identity.dominios.desbloquear"),
        new ApiScope(name: "identity.dominios.bloquear_usuarios"),
        new ApiScope(name: "identity.dominios.desbloquear_usuarios"),
        //Grupos
        new ApiScope(name: "identity.grupos.*"),
        new ApiScope(name: "identity.grupos.listar"),
        new ApiScope(name: "identity.grupos.detalhes"),
        new ApiScope(name: "identity.grupos.usuarios"),
        new ApiScope(name: "identity.grupos.criar"),
        new ApiScope(name: "identity.grupos.criar_subgrupo"),
        new ApiScope(name: "identity.grupos.editar"),
        new ApiScope(name: "identity.grupos.editar_subgrupo"),
        new ApiScope(name: "identity.grupos.remover"),
        new ApiScope(name: "identity.grupos.remover_subgrupo"),
        new ApiScope(name: "identity.grupos.adicionar_usuarios"),
        new ApiScope(name: "identity.grupos.remover_usuarios"),
        //Estabelecimentos
        new ApiScope(name: "identity.estabelecimentos.*"),
        new ApiScope(name: "identity.estabelecimentos.logado"),

        //Catalog API
        new ApiScope(name: "catalog.*"),
    };
}
