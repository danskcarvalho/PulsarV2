using Pulsar.Services.Catalog.API.Application.Queries;
using Pulsar.Services.Catalog.Contracts.Queries;

namespace Pulsar.Services.Catalog.API.Application.BaseTypes;

public static class DIExtensions
{
    public static void AddQueries(this IServiceCollection collection)
    {
        collection.AddTransient<IDenteQueries, DenteQueries>();
        collection.AddTransient<IDiagnosticoQueries, DiagnosticoQueries>();
        collection.AddTransient<IEspecialidadeQueries, EspecialidadeQueries>();
        collection.AddTransient<IEtniaQueries, EtniaQueries>();
        collection.AddTransient<IInepQueries, InepQueries>();
        collection.AddTransient<IMaterialQueries, MaterialQueries>();
        collection.AddTransient<IPrincipioAtivoQueries, PrincipioAtivoQueries>();
        collection.AddTransient<IProcedimentoQueries, ProcedimentoQueries>();
        collection.AddTransient<IRegiaoQueries, RegiaoQueries>();
    }
}
