namespace Pulsar.Services.Identity.API.Utils;

public static class HostEnvironmentExtension
{
    public static bool IsTesting(this IHostEnvironment hostEnvironment)
    {
        return hostEnvironment.EnvironmentName == "Testing";
    }
}
