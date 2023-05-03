using Pulsar.Services.Identity.Contracts.DTOs;
using Pulsar.Services.Identity.Contracts.Queries;

namespace Identity.UnitTests.Queries;

public class MockedEstabelecimentoQueries : IEstabelecimentoQueries
{
    public Task<EstabelecimentoLogadoDTO> GetEstabelecimentoLogado(string estabelecimentoId)
    {
        throw new NotImplementedException();
    }
}
