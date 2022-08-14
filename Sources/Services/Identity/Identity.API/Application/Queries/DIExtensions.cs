namespace Pulsar.Services.Identity.API.Application.Queries;

public static class DIExtensions
{
    public static void AddQueries(this IServiceCollection collection)
    {
        collection.AddTransient<IUsuarioQueries, UsuarioQueries>(); 
    }
}
