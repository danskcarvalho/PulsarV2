using Microsoft.AspNetCore.Mvc;
using Pulsar.Services.PushNotification.API.Utils;

namespace Pulsar.Services.PushNotification.API.Controllers;

[ApiController]
[Route("v2/pushnotifications")]
[Authorize(Policy = "InferAuthenticationSchemes")]
public class PushNotificationController : BaseController
{
    public PushNotificationController(BaseControllerContext context) : base(context)
    {
    }
}
