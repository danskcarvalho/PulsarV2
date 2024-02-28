using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Stores;
using Duende.IdentityServer.Stores.Serialization;
using Microsoft.AspNetCore.Authentication;

namespace Pulsar.Services.Identity.API.Services;

public class PulsarRefreshTokenService : DefaultRefreshTokenService
{
    public PulsarRefreshTokenService(IRefreshTokenStore refreshTokenStore, 
        IProfileService profile, 
        IClock clock,
        PersistentGrantOptions options,
        ILogger<PulsarRefreshTokenService> logger) : base(refreshTokenStore, profile, clock, options, logger)
    {
    }

    protected override Task<bool> AcceptConsumedTokenAsync(RefreshToken refreshToken)
    {
        if (refreshToken != null)
        {
            if (refreshToken.ConsumedTime == null)
                return Task.FromResult(true);
            else if (refreshToken.ConsumedTime.Value >= DateTime.UtcNow.AddMinutes(-5))
                return Task.FromResult(true);
            else
                return Task.FromResult(false);

        }
        else
            return Task.FromResult(false);
    }
}
