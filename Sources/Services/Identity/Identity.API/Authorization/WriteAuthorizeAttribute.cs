using Microsoft.AspNetCore.Authorization;

namespace Pulsar.Services.Identity.API.Authorization;

public class WriteAuthorizeAttribute : AuthorizeAttribute
{
    public WriteAuthorizeAttribute()
    {
        Policy = "Write";
    }
}
