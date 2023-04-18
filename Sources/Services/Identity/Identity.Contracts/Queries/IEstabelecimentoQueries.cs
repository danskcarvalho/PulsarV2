namespace Pulsar.Services.Identity.Contracts.Queries;

public interface IEstabelecimentoQueries
{
    Task<EstabelecimentoLogadoDTO> GetEstabelecimentoLogado(string estabelecimentoId);
}
