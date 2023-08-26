using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Pulsar.Services.Identity.API.Authorization;

public class IdentityCustomPolicyProvider : IAuthorizationPolicyProvider
{
    const string DOMINIO_POLICY_PREFIX = "Dominio_";
    const string ESTABELECIMENTO_POLICY_PREFIX = "Estabelecimento_";
    const string SUPERUSUARIO_POLICY_PREFIX = "SuperUsuario_";
    const string SUPERUSUARIO_OR_DOMINIO_POLICY_PREFIX = "SuperUsuarioOrDominio_";
    const string SCOPE_PREFIX = "Scope_";

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
            var requiredPermissions = policyName.Substring(DOMINIO_POLICY_PREFIX.Length).Split('_', StringSplitOptions.RemoveEmptyEntries);
            var builder = new AuthorizationPolicyBuilder("Bearer", "MockedAuthScheme");
            builder.RequireAuthenticatedUser();
            if (requiredPermissions.Length != 0)
            {
                builder.RequireAssertion(ctx =>
                    ctx.User.HasClaim(c => c.Type == "uad" && c.Value == "true") ||
                    (ctx.User.HasClaim(c => c.Type == "d" && c.Value.IsNotEmpty()) && ctx.User.HasClaim(c => c.Type == "dp" && c.Value.IsNotEmpty() && ContainsPermissions(c.Value, requiredPermissions))));
            }
            else
            {
                builder.RequireAssertion(ctx =>
                    ctx.User.HasClaim(c => c.Type == "uad" && c.Value == "true") ||
                    ctx.User.HasClaim(c => c.Type == "d" && c.Value.IsNotEmpty()));
            }
            return Task.FromResult((AuthorizationPolicy?)builder.Build());
        }
        else if (policyName.StartsWith(ESTABELECIMENTO_POLICY_PREFIX, StringComparison.OrdinalIgnoreCase))
        {
            var requiredPermissions = policyName.Substring(ESTABELECIMENTO_POLICY_PREFIX.Length).Split('_', StringSplitOptions.RemoveEmptyEntries);
            var builder = new AuthorizationPolicyBuilder("Bearer", "MockedAuthScheme");
            builder.RequireAuthenticatedUser();
            if (requiredPermissions.Length != 0)
            {
                builder.RequireAssertion(ctx =>
                    ctx.User.HasClaim(c => c.Type == "e" && c.Value.IsNotEmpty()) && ctx.User.HasClaim(c => c.Type == "ep" && c.Value.IsNotEmpty() && ContainsPermissions(c.Value, requiredPermissions)));
            }
            else
            {
                builder.RequireAssertion(ctx =>
                    ctx.User.HasClaim(c => c.Type == "e" && c.Value.IsNotEmpty()));
            }
            return Task.FromResult((AuthorizationPolicy?)builder.Build());
        }
        else if (policyName.StartsWith(SUPERUSUARIO_POLICY_PREFIX, StringComparison.OrdinalIgnoreCase))
        {
            var builder = new AuthorizationPolicyBuilder("Bearer", "MockedAuthScheme");
            builder.RequireAuthenticatedUser();
            builder.RequireAssertion(ctx => ctx.User.HasClaim(c => c.Type == "uag" && c.Value == "true"));
            return Task.FromResult((AuthorizationPolicy?)builder.Build());
        }
        else if (policyName.StartsWith(SUPERUSUARIO_OR_DOMINIO_POLICY_PREFIX, StringComparison.OrdinalIgnoreCase))
        {
            var requiredPermissions = policyName.Substring(SUPERUSUARIO_OR_DOMINIO_POLICY_PREFIX.Length).Split('_', StringSplitOptions.RemoveEmptyEntries);
            var builder = new AuthorizationPolicyBuilder("Bearer", "MockedAuthScheme");
            builder.RequireAuthenticatedUser();
            if (requiredPermissions.Length != 0)
            {
                builder.RequireAssertion(ctx =>
                    ctx.User.HasClaim(c => c.Type == "uag" && c.Value == "true") ||
                    ctx.User.HasClaim(c => c.Type == "uad" && c.Value == "true") ||
                    (ctx.User.HasClaim(c => c.Type == "d" && c.Value.IsNotEmpty()) && ctx.User.HasClaim(c => c.Type == "dp" && c.Value.IsNotEmpty() && ContainsPermissions(c.Value, requiredPermissions))));
            }
            else
            {
                builder.RequireAssertion(ctx =>
                    ctx.User.HasClaim(c => c.Type == "uag" && c.Value == "true") ||
                    ctx.User.HasClaim(c => c.Type == "uad" && c.Value == "true") ||
                    ctx.User.HasClaim(c => c.Type == "d" && c.Value.IsNotEmpty()));
            }
            return Task.FromResult((AuthorizationPolicy?)builder.Build());
        }
        else if (policyName.StartsWith(SCOPE_PREFIX, StringComparison.OrdinalIgnoreCase))
        {
            var builder = new AuthorizationPolicyBuilder("Bearer", "MockedAuthScheme");
            builder.RequireAuthenticatedUser();
            var claim = policyName.Substring(SCOPE_PREFIX.Length);
            var api = claim.Substring(0, claim.IndexOf('.'));
            var controller = claim.Substring(api.Length + 1).Substring(0, claim.IndexOf('.'));
            var action = claim.Substring(api.Length + controller.Length + 2);
            builder.RequireAssertion(ctx => ctx.User.HasClaim("scope", claim) || ctx.User.HasClaim("scope", $"{api}.*") || ctx.User.HasClaim("scope", $"{api}.{controller}.*"));
            return Task.FromResult((AuthorizationPolicy?)builder.Build());
        }
        else
        {
            var policy = new AuthorizationPolicyBuilder("Bearer", "MockedAuthScheme").RequireAuthenticatedUser().Build();
            return Task.FromResult((AuthorizationPolicy?)policy);
        }
    }

    private bool ContainsPermissions(string permissions, string[] requiredPermission)
    {
        var allPermissions = new HashSet<string>(permissions.Split(',', StringSplitOptions.RemoveEmptyEntries));
        return requiredPermission.All(rp => allPermissions.Contains(rp));
    }
}
