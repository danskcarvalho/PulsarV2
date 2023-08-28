using Identity.IntegrationTests.BaseTypes;
using Identity.IntegrationTests.Utils;
using Pulsar.Services.Identity.Contracts.Commands.Dominios;
using Pulsar.Services.Identity.Contracts.DTOs;
using Pulsar.Services.Shared.DTOs;
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

    [Fact]
    public async Task Listar_Dominios()
    {
        var client = GetClient(Users.Administrador);
        var sortedDominios = IdentityDatabase.Current.Dominios.OrderBy(d => d.Nome, StringComparer.Ordinal).ThenBy(d => d.Id).ToList();
        var offset = 0;

        //act
        var result = await client.GetFromJsonAsync<PaginatedListDTO<DominioListadoDTO>>($"{Get.Listar}{BuildQueryString(new { limit = 20 })}");

        //verify
        Assert.NotNull(result);
        Assert.Equal(20, result.Page.Count);
        Assert.NotNull(result.Next);
        Assert.All(result.Page, d => Assert.NotNull(d.Nome));
        for (int i = 0; i < result.Page.Count; i++)
        {
            Assert.Equal(sortedDominios[i + offset].Id.ToString(), result.Page[i].DominioId);
            Assert.Equal(sortedDominios[i + offset].Nome, result.Page[i].Nome);
        }

        //act again
        result = await client.GetFromJsonAsync<PaginatedListDTO<DominioListadoDTO>>($"{Get.Listar}{BuildQueryString(new { cursor = result.Next, limit = 20 })}");

        //verify
        Assert.NotNull(result);
        Assert.Equal(20, result.Page.Count);
        Assert.NotNull(result.Next);
        Assert.All(result.Page, d => Assert.NotNull(d.Nome));
        offset += 20;
        for (int i = 0; i < result.Page.Count; i++)
        {
            Assert.Equal(sortedDominios[i + offset].Id.ToString(), result.Page[i].DominioId);
            Assert.Equal(sortedDominios[i + offset].Nome, result.Page[i].Nome);
        }
    }

    private static readonly string BaseUrl = "v2/dominios";
    private static class Put
    {
        public static readonly string Criar = $"{BaseUrl}";
    }

    private static class Get
    {
        public static readonly string Listar = $"{BaseUrl}";
    }
}
