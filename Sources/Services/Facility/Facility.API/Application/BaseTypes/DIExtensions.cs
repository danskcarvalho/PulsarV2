namespace Pulsar.Services.Facility.API.Application.BaseTypes;

public static class DIExtensions
{
    public static void AddQueries(this IServiceCollection collection)
    {
        collection.AddTransient<IEstabelecimentoQueries, EstabelecimentosQueries>();
    }
}
