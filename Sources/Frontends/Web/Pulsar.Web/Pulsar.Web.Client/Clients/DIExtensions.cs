﻿using Pulsar.Web.Client.Clients.Identity.Base;
using Pulsar.Web.Client.Clients.Identity.Dominios;
using Pulsar.Web.Client.Clients.Identity.Estabelecimentos;
using Pulsar.Web.Client.Clients.Identity.Usuarios;

namespace Pulsar.Web.Client.Clients;

public static class DIExtensions
{
    public static void AddClients(this IServiceCollection services)
    {
        services.AddTransient<ClientContext>();
        services.AddTransient<IDominioClient, DominioClient>();
        services.AddTransient<IEstabelecimentoClient, EstabelecimentoClient>();
        services.AddTransient<IUsuarioClient, UsuarioClient>();
    }
}