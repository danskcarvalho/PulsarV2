﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pulsar.Services.Identity.Functions.Application.Functions;
using ServiceBus.Migrations.Core;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddSingleton<Migrator>();
    })
    .Build();

var migrator = host.Services.GetRequiredService<Migrator>();

await migrator
    .AddAssembly(typeof(AtualizarEstabelecimentoFN).Assembly)
    .Migrate();