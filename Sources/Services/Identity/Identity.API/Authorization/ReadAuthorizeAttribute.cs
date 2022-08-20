using Microsoft.AspNetCore.Authorization;

namespace Pulsar.Services.Identity.API.Authorization;

public class ReadAuthorizeAttribute : AuthorizeAttribute
{
    public ReadAuthorizeAttribute()
    {
        Policy = "Read";
    }
}
