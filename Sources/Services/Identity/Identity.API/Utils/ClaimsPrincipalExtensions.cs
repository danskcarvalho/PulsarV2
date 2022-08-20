using System.Security.Claims;

namespace Pulsar.Services.Identity.API.Utils;

public static class ClaimsPrincipalExtensions
{
    public static string Id(this ClaimsPrincipal cp)
    {
        var sub = cp.Claims.First(c => c.Type == "sub" || c.Type == ClaimTypes.NameIdentifier);
        var parts = sub.Value.Split('/', StringSplitOptions.RemoveEmptyEntries);
        return parts[2];
    }

    public static string? DominioId(this ClaimsPrincipal cp)
    {
        var sub = cp.Claims.First(c => c.Type == "sub" || c.Type == ClaimTypes.NameIdentifier);
        var parts = sub.Value.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var d = parts[0];
        return d == "_" ? null : d;
    }

    public static string? EstabelecimentoId(this ClaimsPrincipal cp)
    {
        var sub = cp.Claims.First(c => c.Type == "sub" || c.Type == ClaimTypes.NameIdentifier);
        var parts = sub.Value.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var d = parts[1];
        return d == "_" ? null : d;
    }
}
