﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Pulsar.Services.Shared.API.Utils;
using System;
using Pulsar.BuildingBlocks.Utils;

namespace Pulsar.Services.Shared.API.Authorization;

public class CustomPolicyProvider : IAuthorizationPolicyProvider
{
    const string DOMINIO_POLICY_PREFIX = "Dominio_";
    const string ESTABELECIMENTO_POLICY_PREFIX = "Estabelecimento_";
    const string SUPERUSUARIO_POLICY_PREFIX = "SuperUsuario_";
    const string SUPERUSUARIO_OR_DOMINIO_POLICY_PREFIX = "SuperUsuarioOrDominio_";
    const string SCOPE_PREFIX = "Scope_";
    const string INFER_AUTHENTICATION_SCHEMES = "InferAuthenticationSchemes";

    private IWebHostEnvironment _environment;
    private DefaultAuthorizationPolicyProvider BackupPolicyProvider { get; }

    public CustomPolicyProvider(IOptions<AuthorizationOptions> options, IWebHostEnvironment environment)
    {
        BackupPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
        _environment = environment;
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => BackupPolicyProvider.GetDefaultPolicyAsync();

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() => BackupPolicyProvider.GetFallbackPolicyAsync();

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if(policyName == INFER_AUTHENTICATION_SCHEMES)
        {
            var builder = new AuthorizationPolicyBuilder();
            builder.RequireAuthenticatedUser();
            if (_environment.IsTesting())
            {
                builder.AddAuthenticationSchemes("Bearer", "Testing");
            }
            else
            {
                builder.AddAuthenticationSchemes("Bearer");
            }
            return Task.FromResult((AuthorizationPolicy?)builder.Build());
        }
        else if (policyName.StartsWith(DOMINIO_POLICY_PREFIX, StringComparison.OrdinalIgnoreCase))
        {
            var requiredPermissions = policyName.Substring(DOMINIO_POLICY_PREFIX.Length).Split('_', StringSplitOptions.RemoveEmptyEntries);
            var builder = new AuthorizationPolicyBuilder();
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
            var builder = new AuthorizationPolicyBuilder();
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
            var builder = new AuthorizationPolicyBuilder();
            builder.RequireAuthenticatedUser();
            builder.RequireAssertion(ctx => ctx.User.HasClaim(c => c.Type == "uag" && c.Value == "true"));
            return Task.FromResult((AuthorizationPolicy?)builder.Build());
        }
        else if (policyName.StartsWith(SUPERUSUARIO_OR_DOMINIO_POLICY_PREFIX, StringComparison.OrdinalIgnoreCase))
        {
            var requiredPermissions = policyName.Substring(SUPERUSUARIO_OR_DOMINIO_POLICY_PREFIX.Length).Split('_', StringSplitOptions.RemoveEmptyEntries);
            var builder = new AuthorizationPolicyBuilder();
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
            var builder = new AuthorizationPolicyBuilder();
            builder.RequireAuthenticatedUser();
            var claim = policyName.Substring(SCOPE_PREFIX.Length);
            var parts = claim.Split('.', StringSplitOptions.RemoveEmptyEntries);
            var api = parts[0];
            var controller = parts[1];
            var action = parts[2];
            builder.RequireAssertion(ctx => ctx.User.HasClaim("scope", claim) || ctx.User.HasClaim("scope", $"{api}.*") || ctx.User.HasClaim("scope", $"{api}.{controller}.*"));
            return Task.FromResult((AuthorizationPolicy?)builder.Build());
        }
        else
        {
            var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
            return Task.FromResult((AuthorizationPolicy?)policy);
        }
    }

    private bool ContainsPermissions(string permissions, string[] requiredPermission)
    {
        var allPermissions = new HashSet<string>(permissions.Split(',', StringSplitOptions.RemoveEmptyEntries));
        return requiredPermission.All(rp => allPermissions.Contains(rp));
    }
}
