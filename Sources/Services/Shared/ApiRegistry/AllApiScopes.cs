using Duende.IdentityServer.Models;

namespace Pulsar.Services.ApiRegistry;

public static class AllApiScopes
{
    public readonly static ApiScope[] Resources = new ApiScope[]
    {
        //Identity API
        new ApiScope("identity.*"),
        //Convites
        new ApiScope("identity.convites.*"),
        new ApiScope("identity.convites.criar"),
        //Usuarios
        new ApiScope("identity.usuarios.*"),
        new ApiScope("identity.usuarios.listar"),
        new ApiScope("identity.usuarios.logado"),
        new ApiScope("identity.usuarios.dados_basicos"),
        new ApiScope("identity.usuarios.detalhes"),
        new ApiScope("identity.usuarios.bloquear"),
        new ApiScope("identity.usuarios.desbloquear"),
        new ApiScope("identity.usuarios.editar_meus_dados"),
        new ApiScope("identity.usuarios.mudar_minha_senha"),
        new ApiScope("identity.usuarios.mudar_meu_avatar"),
        //Domínios
        new ApiScope("identity.dominios.*"),
        new ApiScope("identity.dominios.listar"),
        new ApiScope("identity.dominios.logado"),
        new ApiScope("identity.dominios.esconder"),
        new ApiScope("identity.dominios.mostrar"),
        new ApiScope("identity.dominios.detalhes"),
        new ApiScope("identity.dominios.usuarios_bloqueados"),
        new ApiScope("identity.dominios.criar"),
        new ApiScope("identity.dominios.editar"),
        new ApiScope("identity.dominios.bloquear"),
        new ApiScope("identity.dominios.desbloquear"),
        new ApiScope("identity.dominios.bloquear_usuarios"),
        new ApiScope("identity.dominios.desbloquear_usuarios"),
        //Grupos
        new ApiScope("identity.grupos.*"),
        new ApiScope("identity.grupos.listar"),
        new ApiScope("identity.grupos.detalhes"),
        new ApiScope("identity.grupos.usuarios"),
        new ApiScope("identity.grupos.criar"),
        new ApiScope("identity.grupos.criar_subgrupo"),
        new ApiScope("identity.grupos.editar"),
        new ApiScope("identity.grupos.editar_subgrupo"),
        new ApiScope("identity.grupos.remover"),
        new ApiScope("identity.grupos.remover_subgrupo"),
        new ApiScope("identity.grupos.adicionar_usuarios"),
        new ApiScope("identity.grupos.remover_usuarios"),
        //Estabelecimentos
        new ApiScope("identity.estabelecimentos.*"),
        new ApiScope("identity.estabelecimentos.logado"),

        //Catalog API
        new ApiScope("catalog.*"),
        // Dentes
        new ApiScope("catalog.dentes.*"),
        new ApiScope("catalog.dentes.listar"),
        // Diagnósticos
        new ApiScope("catalog.diagnosticos.*"),
        new ApiScope("catalog.diagnosticos.listar"),
        // Especialidades
        new ApiScope("catalog.especialidades.*"),
        new ApiScope("catalog.especialidades.listar"),
        // Etnias
        new ApiScope("catalog.etnias.*"),
        new ApiScope("catalog.etnias.listar"),
        // Ineps
        new ApiScope("catalog.ineps.*"),
        new ApiScope("catalog.ineps.listar"),
        // Materiais
        new ApiScope("catalog.materiais.*"),
        new ApiScope("catalog.materiais.listar"),
        // Principios Ativos
        new ApiScope("catalog.principiosativos.*"),
        new ApiScope("catalog.principiosativos.listar"),
        // Procedimentos
        new ApiScope("catalog.procedimentos.*"),
        new ApiScope("catalog.procedimentos.listar"),
        // Regiões
        new ApiScope("catalog.regioes.*"),
        new ApiScope("catalog.regioes.listar"),

        //Facility API
        new ApiScope("facility.*"),
	};
}
