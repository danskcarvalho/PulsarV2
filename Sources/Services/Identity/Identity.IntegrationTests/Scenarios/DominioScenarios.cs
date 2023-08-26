using Identity.IntegrationTests.BaseTypes;
using Identity.IntegrationTests.Utils;
using Pulsar.Services.Identity.Contracts.Commands.Dominios;
using System.Net.Http.Json;

namespace Identity.IntegrationTests.Scenarios;

public class DominioScenarios : IdentityScenarios
{
    [Fact]
    public async Task Criar_Dominio()
    {
        var client = GetClient(Users.Administrador);

        // act
        var result = await client.PutAsJsonAsync(Put.Criar, new CriarDominioCmd()
        {
            Nome = "DOMINIO_NOME"
        });

        //verify
        result.EnsureSuccessStatusCode();
    }

    private static readonly string BaseUrl = "v2/dominios";
    private static class Put
    {
        public static readonly string Criar = $"{BaseUrl}";
    }
}
