using Pulsar.Services.Identity.Domain.Aggregates.Usuarios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.IntegrationTests.Utils
{
    public static class Users
    {
        public static readonly TestUser Administrador = new TestUser(Usuario.SuperUsuarioId.ToString(), null, null);
    }
}
