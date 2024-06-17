using Pulsar.Services.Shared.Enumerations;

namespace Pulsar.Services.Catalog.Contracts.Queries;

public interface IProcedimentoQueries
{
    public Task<List<ProcedimentoDTO>> Find(string? filtro, Sexo? sexo, Complexidade? complexidade, int? idadeEmDias, bool? procedimentoAb);
}
