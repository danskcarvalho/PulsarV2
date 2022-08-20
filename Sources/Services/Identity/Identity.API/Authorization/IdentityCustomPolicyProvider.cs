using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Pulsar.Services.Identity.API.Authorization;

public class IdentityCustomPolicyProvider : IAuthorizationPolicyProvider
{
    const string DOMINIO_POLICY_PREFIX = "Dominio_";
    const string ESTABELECIMENTO_POLICY_PREFIX = "Estabelecimento_";
    const string SUPERUSUARIO_POLICY_PREFIX = "SuperUsuario_";
    const string SUPERUSUARIO_OR_DOMINIO_POLICY_PREFIX = "SuperUsuarioOrDominio_";

    private DefaultAuthorizationPolicyProvider BackupPolicyProvider { get; }

    public IdentityCustomPolicyProvider(IOptions<AuthorizationOptions> options)
    {
        BackupPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => BackupPolicyProvider.GetDefaultPolicyAsync();

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() => BackupPolicyProvider.GetFallbackPolicyAsync();

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (policyName.StartsWith(DOMINIO_POLICY_PREFIX, StringComparison.OrdinalIgnoreCase))
        {
            var requiredPermission = policyName.Substring(DOMINIO_POLICY_PREFIX.Length).Split('_', StringSplitOptions.RemoveEmptyEntries);
            var builder = new AuthorizationPolicyBuilder("Bearer");
            builder.RequireAuthenticatedUser();
            builder.RequireAssertion(ctx =>
                ctx.User.HasClaim(c => c.Type == "uad" && c.Value == "true") ||
                (ctx.User.HasClaim(c => c.Type == "d" && c.Value.IsNotEmpty()) && ctx.User.HasClaim(c => c.Type == "dp" && c.Value.IsNotEmpty() && ContainsPermissions(c.Value, requiredPermission))));
            return Task.FromResult((AuthorizationPolicy?)builder.Build());
        }
        else if (policyName.StartsWith(ESTABELECIMENTO_POLICY_PREFIX, StringComparison.OrdinalIgnoreCase))
        {
            var requiredPermission = policyName.Substring(ESTABELECIMENTO_POLICY_PREFIX.Length).Split('_', StringSplitOptions.RemoveEmptyEntries);
            var builder = new AuthorizationPolicyBuilder("Bearer");
            builder.RequireAuthenticatedUser();
            builder.RequireAssertion(ctx =>
                (ctx.User.HasClaim(c => c.Type == "e" && c.Value.IsNotEmpty()) && ctx.User.HasClaim(c => c.Type == "ep" && c.Value.IsNotEmpty() && ContainsPermissions(c.Value, requiredPermission))));
            return Task.FromResult((AuthorizationPolicy?)builder.Build());
        }
        else if (policyName.StartsWith(SUPERUSUARIO_POLICY_PREFIX, StringComparison.OrdinalIgnoreCase))
        {
            var builder = new AuthorizationPolicyBuilder("Bearer");
            builder.RequireAuthenticatedUser();
            builder.RequireAssertion(ctx => ctx.User.HasClaim(c => c.Type == "uag" && c.Value == "true"));
            return Task.FromResult((AuthorizationPolicy?)builder.Build());
        }
        else if (policyName.StartsWith(SUPERUSUARIO_OR_DOMINIO_POLICY_PREFIX, StringComparison.OrdinalIgnoreCase))
        {
            var requiredPermission = policyName.Substring(SUPERUSUARIO_OR_DOMINIO_POLICY_PREFIX.Length).Split('_', StringSplitOptions.RemoveEmptyEntries);
            var builder = new AuthorizationPolicyBuilder("Bearer");
            builder.RequireAuthenticatedUser();
            builder.RequireAssertion(ctx =>
                ctx.User.HasClaim(c => c.Type == "uag" && c.Value == "true") ||
                ctx.User.HasClaim(c => c.Type == "uad" && c.Value == "true") ||
                (ctx.User.HasClaim(c => c.Type == "d" && c.Value.IsNotEmpty()) && ctx.User.HasClaim(c => c.Type == "dp" && c.Value.IsNotEmpty() && ContainsPermissions(c.Value, requiredPermission))));
            return Task.FromResult((AuthorizationPolicy?)builder.Build());
        }
        else if (policyName == "Read")
        {
            var builder = new AuthorizationPolicyBuilder("Bearer");
            builder.RequireAuthenticatedUser();
            builder.RequireClaim("scope", "identity.read");
            return Task.FromResult((AuthorizationPolicy?)builder.Build());
        }
        else if (policyName == "Write")
        {
            var builder = new AuthorizationPolicyBuilder("Bearer");
            builder.RequireAuthenticatedUser();
            builder.RequireClaim("scope", "identity.write");
            return Task.FromResult((AuthorizationPolicy?)builder.Build());
        }
        else
        {
            var policy = new AuthorizationPolicyBuilder("Bearer").RequireAuthenticatedUser().Build();
            return Task.FromResult((AuthorizationPolicy?)policy);
        }
    }

    private bool ContainsPermissions(string permissions, string[] requiredPermission)
    {
        var allPermissions = new HashSet<string>(permissions.Split(',', StringSplitOptions.RemoveEmptyEntries));
        return requiredPermission.All(rp => allPermissions.Contains(rp));
    }
}
