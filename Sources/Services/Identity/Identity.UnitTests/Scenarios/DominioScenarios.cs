using MongoDB.Bson;
using Pulsar.Services.Identity.Contracts.Utils;
using Pulsar.Services.Identity.Domain.Aggregates.Dominios;

namespace Identity.UnitTests.Scenarios;

public class DominioScenarios : IdentityScenarios
{
    [Fact]
    public async Task Bloquear_e_Desbloquear_Dominio()
    {
        var dominioId = ObjectId.Parse(IdentityDatabase.DominioPadraoId);
        var controller = CreateController<DominioController>(Users.Admin);
        var actionResult = await controller.Bloquear(IdentityDatabase.DominioPadraoId);

        Assert.Equal((int)System.Net.HttpStatusCode.OK, (actionResult.Result as OkObjectResult)?.StatusCode);

        var dominio = await Dominios.FindAsync(x => x.Id == dominioId).FirstOrDefaultAsync();
        Assert.NotNull(dominio);
        Assert.False(dominio.IsAtivo);

        actionResult = await controller.Desbloquear(IdentityDatabase.DominioPadraoId);

        Assert.Equal((int)System.Net.HttpStatusCode.OK, (actionResult.Result as OkObjectResult)?.StatusCode);

        dominio = await Dominios.FindAsync(x => x.Id == dominioId).FirstOrDefaultAsync();
        Assert.NotNull(dominio);
        Assert.True(dominio.IsAtivo);
    }

    [Fact]
    public async Task Bloquear_e_Desbloquear_Usuarios()
    {
        var dominioId = ObjectId.Parse(IdentityDatabase.DominioPadraoId);
        var alexiaId = ObjectId.Parse(IdentityDatabase.AlexiaUserId);
        var controller = CreateController<DominioController>(Users.Admin);
        var actionResult = await controller.BloquearUsuarios(dominioId.ToString(), new Pulsar.Services.Identity.Contracts.Commands.Dominios.BloquearOuDesbloquearUsuariosNoDominioCmd()
        {
            UsuarioIds = new List<string> { IdentityDatabase.AlexiaUserId }
        });
        Assert.Equal((int)System.Net.HttpStatusCode.OK, (actionResult.Result as OkObjectResult)?.StatusCode);

        var alexia = await Usuarios.FindAsync(x => x.Id == alexiaId).FirstOrDefaultAsync();
        Assert.NotNull(alexia);
        Assert.Contains(dominioId, alexia.DominiosBloqueados);

        actionResult = await controller.DesbloquearUsuarios(dominioId.ToString(), new Pulsar.Services.Identity.Contracts.Commands.Dominios.BloquearOuDesbloquearUsuariosNoDominioCmd()
        {
            UsuarioIds = new List<string> { IdentityDatabase.AlexiaUserId }
        });
        Assert.Equal((int)System.Net.HttpStatusCode.OK, (actionResult.Result as OkObjectResult)?.StatusCode);

        alexia = await Usuarios.FindAsync(x => x.Id == alexiaId).FirstOrDefaultAsync();
        Assert.NotNull(alexia);
        Assert.DoesNotContain(dominioId, alexia.DominiosBloqueados);
    }

    [Fact]
    public async Task Bloquear_Usuario_Administrador()
    {
        var dominioId = ObjectId.Parse(IdentityDatabase.DominioPadraoId);
        var samanthaId = ObjectId.Parse(IdentityDatabase.SamanthaUserId);
        var controller = CreateController<DominioController>(Users.Admin);
        var exception = await Assert.ThrowsAsync<IdentityDomainException>(async () => await controller.BloquearUsuarios(dominioId.ToString(), new Pulsar.Services.Identity.Contracts.Commands.Dominios.BloquearOuDesbloquearUsuariosNoDominioCmd()
        {
            UsuarioIds = new List<string> { IdentityDatabase.SamanthaUserId }
        }));
        Assert.Equal(ExceptionKey.UsuarioAdministradorNaoPodeSerBloqueadoDominio, exception.Key);
    }

    [Fact]
    public async Task Editar_Dominio()
    {
        var dominioId = ObjectId.Parse(IdentityDatabase.DominioPadraoId);
        var samanthaId = ObjectId.Parse(IdentityDatabase.SamanthaUserId);
        var controller = CreateController<DominioController>(Users.Admin);
        var actionResult = await controller.Editar(dominioId.ToString(), new Pulsar.Services.Identity.Contracts.Commands.Dominios.EditarDominioCmd()
        {
            UsuarioAdministradorId = samanthaId.ToString(),
            Nome = "PADRÃO 2"
        });
        Assert.Equal((int)System.Net.HttpStatusCode.OK, (actionResult.Result as OkObjectResult)?.StatusCode);

        var dominio = await Dominios.FindAsync(x => x.Id == dominioId).FirstOrDefaultAsync();
        Assert.NotNull(dominio);
        Assert.Equal("PADRÃO 2", dominio.Nome);

        var exception = await Assert.ThrowsAsync<IdentityDomainException>(async () => await controller.Editar(dominioId.ToString(), new Pulsar.Services.Identity.Contracts.Commands.Dominios.EditarDominioCmd()
        {
            UsuarioAdministradorId = IdentityDatabase.AdminUserId,
            Nome = "PADRÃO"
        }));
        Assert.Equal(ExceptionKey.SuperUsuarioNaoPodeAdministrarDominio, exception.Key);
    }

    [Fact]
    public async Task Mudar_Usuario_Administrador_para_Usuario_Bloqueado()
    {
        var dominioId = ObjectId.Parse(IdentityDatabase.DominioPadraoId);
        var alexiaId = ObjectId.Parse(IdentityDatabase.AlexiaUserId);
        var controller = CreateController<DominioController>(Users.Admin);
        var actionResult = await controller.BloquearUsuarios(dominioId.ToString(), new Pulsar.Services.Identity.Contracts.Commands.Dominios.BloquearOuDesbloquearUsuariosNoDominioCmd()
        {
            UsuarioIds = new List<string> { IdentityDatabase.AlexiaUserId }
        });
        Assert.Equal((int)System.Net.HttpStatusCode.OK, (actionResult.Result as OkObjectResult)?.StatusCode);

        var exception = await Assert.ThrowsAsync<IdentityDomainException>(async () => await controller.Editar(dominioId.ToString(), new Pulsar.Services.Identity.Contracts.Commands.Dominios.EditarDominioCmd()
        {
            UsuarioAdministradorId = IdentityDatabase.AlexiaUserId,
            Nome = "PADRÃO"
        }));
        Assert.Equal(ExceptionKey.UsuarioAdministradorIsBloqueadoDominio, exception.Key);
    }

    [Fact]
    public async Task Criar_Dominio()
    {
        var controller = CreateController<DominioController>(Users.Admin);
        var actionResult = await controller.Criar(new Pulsar.Services.Identity.Contracts.Commands.Dominios.CriarDominioCmd()
        {
            Nome = "DOMINIO TESTE"
        });
        Assert.Equal((int)System.Net.HttpStatusCode.OK, (actionResult.Result as OkObjectResult)?.StatusCode);
        var novoId = (actionResult.Result as OkObjectResult)?.Value as CreatedCommandResult;
        Assert.NotNull(novoId);

        var dominio = await Dominios.FindAsync(x => x.Id == ObjectId.Parse(novoId.Id)).FirstOrDefaultAsync();
        Assert.NotNull(dominio);
        Assert.Equal("DOMINIO TESTE", dominio.Nome);

        var allDominios = await Dominios.FindAsync(x => true).ToListAsync();
        Assert.True(allDominios.Count == 2);
    }

    [Fact]
    public async Task Esconder_e_Mostrar_Dominio()
    {
        var dominioId = ObjectId.Parse(IdentityDatabase.DominioPadraoId);
        var controller = CreateController<DominioController>(Users.Admin);

        var actionResult = await controller.Esconder(IdentityDatabase.DominioPadraoId);
        Assert.Equal((int)System.Net.HttpStatusCode.OK, (actionResult.Result as OkObjectResult)?.StatusCode);

        var dominio = await Dominios.FindAsync(x => x.Id == dominioId).FirstOrDefaultAsync();
        Assert.NotNull(dominio);
        Assert.NotNull(dominio.AuditInfo);
        Assert.NotNull(dominio.AuditInfo.EscondidoEm);
        Assert.NotNull(dominio.AuditInfo.EscondidoPorUsuarioId);

        actionResult = await controller.Mostrar(IdentityDatabase.DominioPadraoId);
        Assert.Equal((int)System.Net.HttpStatusCode.OK, (actionResult.Result as OkObjectResult)?.StatusCode);

        dominio = await Dominios.FindAsync(x => x.Id == dominioId).FirstOrDefaultAsync();
        Assert.NotNull(dominio);
        Assert.NotNull(dominio.AuditInfo);
        Assert.Null(dominio.AuditInfo.EscondidoEm);
        Assert.Null(dominio.AuditInfo.EscondidoPorUsuarioId);
    }
}
