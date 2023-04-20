using Duende.IdentityServer.Models;
using Duende.IdentityServer.Stores;
using Microsoft.AspNetCore.Authentication;

namespace Pulsar.Services.Identity.API.Services;

public class PulsarRefreshTokenService : DefaultRefreshTokenService
{
    public PulsarRefreshTokenService(IRefreshTokenStore refreshTokenStore, IProfileService profile, ISystemClock clock, ILogger<DefaultRefreshTokenService> logger) : base(refreshTokenStore, profile, clock, logger)
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
