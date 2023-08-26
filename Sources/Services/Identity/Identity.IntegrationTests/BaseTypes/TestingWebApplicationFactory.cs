extern alias Migrations;
extern alias API;

using Identity.IntegrationTests.Utils;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pulsar.BuildingBlocks.Migrations;
using System.Net.Http.Headers;

namespace Identity.IntegrationTests.BaseTypes;

public class TestingWebApplicationFactory : WebApplicationFactory<API::Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.AddMigrationsWithoutDatabaseMachinery();
        });
        builder.UseEnvironment("Testing");
    }

    public async Task RunMigrations()
    {
        var migrations = this.Services.GetRequiredService<MigrationRunner>();
        await migrations.Run(typeof(Migrations::Program).Assembly, isTestingEnvironment: true);
    }

    public HttpClient GetClient(TestUser user)
    {
        var client = this.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddAuthentication(defaultScheme: "Testing")
                    .AddScheme<MockedAuthOptions, MockedAuthHandler>(
                        "Testing", options =>
                        {
                            options.UsuarioId = user.Id;
                            options.DominioId = user.DominioId;
                            options.EstabelecimentoId = user.EstabelecimentoId;
                        });
            });
        })
        .CreateClient();

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(scheme: "Testing");
        return client;
    }
}
