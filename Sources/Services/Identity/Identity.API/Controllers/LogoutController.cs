using Duende.IdentityServer;
using Duende.IdentityServer.Services;
using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualBasic;
using Polly;
using System.Security.Claims;

namespace Pulsar.Services.Identity.API.Controllers;

[ApiController]
[Route("v1/logout")]
[ApiExplorerSettings(IgnoreApi = true)]
public class LogoutController : ControllerBase
{
    private readonly ILogger<LogoutController> _logger;
    private readonly IIdentityServerInteractionService _interaction;

    public LogoutController(ILogger<LogoutController> logger, IIdentityServerInteractionService interaction)
    {
        _logger = logger;
        _interaction = interaction;
    }

    [Route("try")]
    [HttpPost]
    public async Task<ActionResult<LogoutResultDTO>> TryLogout([FromBody] string? logoutId)
    {
        if (User.Identity == null || User.Identity.IsAuthenticated == false)
        {
            // if the user is not authenticated, then just show logged out page
            return await Logout(logoutId);
        }

        //Test for Xamarin. 
        var context = await _interaction.GetLogoutContextAsync(logoutId);
        if (context?.ShowSignoutPrompt == false)
        {
            //it's safe to automatically sign-out
            return await Logout(logoutId);
        }

        // show the logout prompt. this prevents attacks where the user
        // is automatically signed out by another malicious web page.
        return Ok(new LogoutResultDTO()
        {
            ShowSignoutPrompt = true
        });
    }

    public async Task<ActionResult<LogoutResultDTO>> Logout([FromBody] string? logoutId)
    {
        var idp = User?.FindFirst(JwtClaimTypes.IdentityProvider)?.Value;

        if (idp != null && idp != IdentityServerConstants.LocalIdentityProvider)
        {
            if (logoutId == null)
            {
                // if there's no current logout context, we need to create one
                // this captures necessary info from the current logged in user
                // before we signout and redirect away to the external IdP for signout
                logoutId = await _interaction.CreateLogoutContextAsync();
            }

            string url = "/account/logout?logoutId=" + logoutId;

            try
            {

                // hack: try/catch to handle social providers that throw
                await HttpContext.SignOutAsync(idp, new AuthenticationProperties
                {
                    RedirectUri = url
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LOGOUT ERROR: {ExceptionMessage}", ex.Message);
            }
        }

        // delete authentication cookie
        await HttpContext.SignOutAsync();
        await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);

        // get context information (client name, post logout redirect URI and iframe for federated signout)
        var logout = await _interaction.GetLogoutContextAsync(logoutId);

        return Ok(new LogoutResultDTO()
        {
            ShowSignoutPrompt = false,
            ClientName = string.IsNullOrEmpty(logout?.ClientName) ? logout?.ClientId : logout?.ClientName,
            PostLogoutRedirectUri = logout?.PostLogoutRedirectUri,
            SignOutIFrameUrl = logout?.SignOutIFrameUrl,
            LogoutId = logoutId
        });
    }
}
