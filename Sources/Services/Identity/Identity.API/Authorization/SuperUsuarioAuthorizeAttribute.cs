using Microsoft.AspNetCore.Authorization;
using System.Globalization;

namespace Pulsar.Services.Identity.API.Authorization;

public class SuperUsuarioAuthorizeAttribute : AuthorizeAttribute
{
    public SuperUsuarioAuthorizeAttribute()
    {
        Policy = "SuperUsuario_";
    }
}
