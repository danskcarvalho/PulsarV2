using Pulsar.Services.Shared.Enumerations;

namespace Pulsar.Services.Catalog.Contracts.Queries;

public interface IDiagnosticoQueries
{
    public Task<List<DiagnosticoDTO>> Find(string? filtro, Sexo? sexo, TipoDiagnostico? tipo);
}
