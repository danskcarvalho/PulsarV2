namespace Pulsar.Services.Identity.API.Application.BaseTypes;

public static class DIExtensions
{
    public static void AddQueries(this IServiceCollection collection)
    {
        collection.AddTransient<IUsuarioQueries, UsuarioQueries>();
    }
}
