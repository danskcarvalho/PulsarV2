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
[Route("v2/logout")]
[ApiExplorerSettings(IgnoreApi = true)]
public class LogoutController : IdentityController
{ 
    private readonly IIdentityServerInteractionService _interaction;

    public LogoutController(IIdentityServerInteractionService interaction, IdentityControllerContext context) : base(context)
    {
        _interaction = interaction;
    }

    [Route("try")]
    [HttpPost]
    public async Task<ActionResult<LogoutResultDTO>> TryLogout([FromBody] LogoutDTO logout)
    {
        if (User == null || User.Identity == null || User.Identity.IsAuthenticated == false)
        {
            // if the user is not authenticated, then just show logged out page
            return Ok(new LogoutResultDTO()
            {
                NoUser = true
            });
        }

        //Test for Xamarin. 
        var context = await _interaction.GetLogoutContextAsync(logout.LogoutId);
        if (context?.ShowSignoutPrompt == false)
        {
            //it's safe to automatically sign-out
            return await Logout(logout);
        }

        // show the logout prompt. this prevents attacks where the user
        // is automatically signed out by another malicious web page.
        return Ok(new LogoutResultDTO()
        {
            ShowSignoutPrompt = true
        });
    }

    public async Task<ActionResult<LogoutResultDTO>> Logout([FromBody] LogoutDTO lougoutModel)
    {
        if (User?.Identity?.IsAuthenticated == true)
        {
            // delete local authentication cookie
            await HttpContext.SignOutAsync();
        }


        // get context information (client name, post logout redirect URI and iframe for federated signout)
        var logout = await _interaction.GetLogoutContextAsync(lougoutModel.LogoutId);

        return Ok(new LogoutResultDTO()
        {
            ShowSignoutPrompt = false,
            ClientName = string.IsNullOrEmpty(logout?.ClientName) ? logout?.ClientId : logout?.ClientName,
            PostLogoutRedirectUri = logout?.PostLogoutRedirectUri,
            SignOutIFrameUrl = logout?.SignOutIFrameUrl,
            LogoutId = lougoutModel.LogoutId
        });
    }
}
