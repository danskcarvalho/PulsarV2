using MongoDB.Bson;
using Pulsar.Services.Identity.Contracts.Utils;
using Pulsar.Services.Identity.Domain.Aggregates.Grupos;

namespace Identity.UnitTests.Scenarios;

public class GrupoScenarios : IdentityScenarios
{
    [Fact]
    public async  Task Criar_Grupo()
    {
        var controller = CreateController<GrupoController>(Users.Samantha);
        var actionResult = await controller.Criar(new Pulsar.Services.Identity.Contracts.Commands.Grupos.CriarGrupoCmd()
        {
            Nome = "GRUPO 1"
        });
        Assert.Equal((int)System.Net.HttpStatusCode.OK, (actionResult.Result as OkObjectResult)?.StatusCode);
        var novoId = (actionResult.Result as OkObjectResult)?.Value as CreatedCommandResult;
        Assert.NotNull(novoId);

        var grupo = await Grupos.FindAsync(x => x.Id == ObjectId.Parse(novoId.Id)).FirstOrDefaultAsync();
        Assert.NotNull(grupo);
        Assert.Equal("GRUPO 1", grupo.Nome);
        Assert.Equal(IdentityDatabase.DominioPadraoId, grupo.DominioId.ToString());
    }

    [Fact]
    public async Task Criar_Grupo_e_Subgrupo()
    {
        var controller = CreateController<GrupoController>(Users.Samantha);
        var actionResult = await controller.Criar(new Pulsar.Services.Identity.Contracts.Commands.Grupos.CriarGrupoCmd()
        {
            Nome = "GRUPO 1"
        });
        Assert.Equal((int)System.Net.HttpStatusCode.OK, (actionResult.Result as OkObjectResult)?.StatusCode);
        var novoId = (actionResult.Result as OkObjectResult)?.Value as CreatedCommandResult;
        Assert.NotNull(novoId);

        actionResult = await controller.CriarSubgrupo(novoId.Id, new Pulsar.Services.Identity.Contracts.Commands.Grupos.CriarSubGrupoCmd()
        {
            Nome = "SUB GRUPO 1",
            PermissoesDominio = new List<PermissoesDominio> { PermissoesDominio.ConvidarUsuario },
            PermissoesEstabelecimento = new List<Pulsar.Services.Identity.Contracts.Commands.Grupos.CriarSubGrupoCmd.PermissoesEstabelecimentoOuRede>
            {
                new Pulsar.Services.Identity.Contracts.Commands.Grupos.CriarSubGrupoCmd.PermissoesEstabelecimentoOuRede()
                {
                    EstabelecimentoId = IdentityDatabase.EstabelecimentoPadraoId,
                    Permissoes = new List<PermissoesEstabelecimento> { PermissoesEstabelecimento.EditarAgendas }
                },
                new Pulsar.Services.Identity.Contracts.Commands.Grupos.CriarSubGrupoCmd.PermissoesEstabelecimentoOuRede()
                {
                    RedeEstabelecimentosId = IdentityDatabase.RedeEstabelecimentosPadraoId,
                    Permissoes = new List<PermissoesEstabelecimento> { PermissoesEstabelecimento.EditarEstabelecimento }
                }
            }
        });
        Assert.Equal((int)System.Net.HttpStatusCode.OK, (actionResult.Result as OkObjectResult)?.StatusCode);

        var grupo = await Grupos.FindAsync(x => x.Id == ObjectId.Parse(novoId.Id)).FirstOrDefaultAsync();
        Assert.NotNull(grupo);
        Assert.Single(grupo.SubGrupos);
        Assert.Equal("SUB GRUPO 1", grupo.SubGrupos[0].Nome);
        Assert.Single(grupo.SubGrupos[0].PermissoesDominio);
        Assert.Contains(PermissoesDominio.ConvidarUsuario, grupo.SubGrupos[0].PermissoesDominio);
        Assert.Equal(2, grupo.SubGrupos[0].PermissoesEstabelecimentos.Count);

        Assert.Equal(1, grupo.SubGrupos[0].PermissoesEstabelecimentos.Count(x => x.Seletor.EstabelecimentoId.HasValue));
        foreach (var pe in grupo.SubGrupos[0].PermissoesEstabelecimentos.Where(x => x.Seletor.EstabelecimentoId.HasValue))
        {
            Assert.Equal(IdentityDatabase.EstabelecimentoPadraoId, pe.Seletor.EstabelecimentoId?.ToString());
            Assert.Single(pe.Permissoes);
            Assert.Equal(PermissoesEstabelecimento.EditarAgendas, pe.Permissoes.First());
        }

        Assert.Equal(1, grupo.SubGrupos[0].PermissoesEstabelecimentos.Count(x => x.Seletor.RedeEstabelecimentoId.HasValue));
        foreach (var pe in grupo.SubGrupos[0].PermissoesEstabelecimentos.Where(x => x.Seletor.RedeEstabelecimentoId.HasValue))
        {
            Assert.Equal(IdentityDatabase.RedeEstabelecimentosPadraoId, pe.Seletor.RedeEstabelecimentoId?.ToString());
            Assert.Single(pe.Permissoes);
            Assert.Equal(PermissoesEstabelecimento.EditarEstabelecimento, pe.Permissoes.First());
        }
    }

    [Fact]
    public async Task Editar_Grupo_e_Subgrupo()
    {
        var controller = CreateController<GrupoController>(Users.Samantha);
        var grupo = await CriarGrupoSubGrupos();

        var actionResult = await controller.Editar(grupo.Id.ToString(), new Pulsar.Services.Identity.Contracts.Commands.Grupos.EditarGrupoCmd()
        {
            Nome = "GRUPO EDITADO"
        });
        Assert.Equal((int)System.Net.HttpStatusCode.OK, (actionResult.Result as OkObjectResult)?.StatusCode);

        grupo = await Grupos.FindAsync(x => x.Id == grupo.Id).FirstOrDefaultAsync();
        Assert.NotNull(grupo);
        Assert.Equal("GRUPO EDITADO", grupo.Nome);

        var subgrupoId = grupo.SubGrupos[0].SubGrupoId;
        actionResult = await controller.EditarSubgrupo(grupo.Id.ToString(), subgrupoId.ToString(), new Pulsar.Services.Identity.Contracts.Commands.Grupos.EditarSubGrupoCmd()
        {
            Nome = "MEU SUBGRUPO",
            PermissoesDominio = new List<PermissoesDominio>(),
            PermissoesEstabelecimento = new List<Pulsar.Services.Identity.Contracts.Commands.Grupos.EditarSubGrupoCmd.PermissoesEstabelecimentoOuRede>
            {
                new Pulsar.Services.Identity.Contracts.Commands.Grupos.EditarSubGrupoCmd.PermissoesEstabelecimentoOuRede()
                {
                    EstabelecimentoId = IdentityDatabase.EstabelecimentoPadraoId,
                    Permissoes = new List<PermissoesEstabelecimento> { PermissoesEstabelecimento.EditarAgendas }
                },
                new Pulsar.Services.Identity.Contracts.Commands.Grupos.EditarSubGrupoCmd.PermissoesEstabelecimentoOuRede()
                {
                    RedeEstabelecimentosId = IdentityDatabase.RedeEstabelecimentosPadraoId,
                    Permissoes = new List<PermissoesEstabelecimento> { PermissoesEstabelecimento.EditarEstabelecimento }
                }
            }
        });
        Assert.Equal((int)System.Net.HttpStatusCode.OK, (actionResult.Result as OkObjectResult)?.StatusCode);

        grupo = await Grupos.FindAsync(x => x.Id == grupo.Id).FirstOrDefaultAsync();
        Assert.NotNull(grupo);
        var subgrupo = grupo.SubGrupos.First(sg => sg.SubGrupoId == subgrupoId);
        Assert.Equal("MEU SUBGRUPO", subgrupo.Nome);
        Assert.Empty(subgrupo.PermissoesDominio);
    }

    [Fact]
    public async Task Remover_Subgrupo()
    {
        var controller = CreateController<GrupoController>(Users.Samantha);
        var grupo = await CriarGrupoSubGrupos();
        var actionResult = await controller.RemoverSubgrupo(grupo.Id.ToString(), grupo.SubGrupos.First().SubGrupoId.ToString());
        Assert.Equal((int)System.Net.HttpStatusCode.OK, (actionResult.Result as OkObjectResult)?.StatusCode);

        grupo = await Grupos.FindAsync(x => x.Id == grupo.Id).FirstOrDefaultAsync();
        Assert.NotNull(grupo);
        Assert.Empty(grupo.SubGrupos);
    }

    [Fact]
    public async Task Remover_Grupo()
    {
        var controller = CreateController<GrupoController>(Users.Samantha);
        var grupo = await CriarGrupoSubGrupos();
        var actionResult = await controller.Remover(grupo.Id.ToString());
        Assert.Equal((int)System.Net.HttpStatusCode.OK, (actionResult.Result as OkObjectResult)?.StatusCode);

        grupo = await Grupos.FindAsync(x => x.Id == grupo.Id).FirstOrDefaultAsync();
        Assert.NotNull(grupo);
        Assert.NotNull(grupo.AuditInfo.RemovidoEm);
        Assert.NotNull(grupo.AuditInfo.RemovidoPorUsuarioId);
    }

    [Fact]
    public async Task Adicionar_e_Remover_Usuarios()
    {
        var controller = CreateController<GrupoController>(Users.Samantha);
        var grupo = await CriarGrupoSubGrupos();
        var subgrupoId = grupo.SubGrupos.First().SubGrupoId;
        var subgrupo = grupo.SubGrupos.First(x => x.SubGrupoId == subgrupoId);
        Assert.Empty(subgrupo.UsuarioIds);

        var actionResult = await controller.AdicionarUsuarios(grupo.Id.ToString(), subgrupoId.ToString(), new Pulsar.Services.Identity.Contracts.Commands.Grupos.AdicionarUsuariosSubGrupoCmd()
        {
            UsuarioIds = new List<string> { IdentityDatabase.AlexiaUserId }
        });
        Assert.Equal((int)System.Net.HttpStatusCode.OK, (actionResult.Result as OkObjectResult)?.StatusCode);

        grupo = await Grupos.FindAsync(x => x.Id == grupo.Id).FirstOrDefaultAsync();
        Assert.NotNull(grupo);
        subgrupo = grupo.SubGrupos.First(x => x.SubGrupoId == subgrupoId);
        Assert.NotEmpty(subgrupo.UsuarioIds);
        Assert.Contains(ObjectId.Parse(IdentityDatabase.AlexiaUserId), subgrupo.UsuarioIds);

        actionResult = await controller.RemoverUsuarios(grupo.Id.ToString(), subgrupoId.ToString(), new Pulsar.Services.Identity.Contracts.Commands.Grupos.RemoverUsuariosSubGrupoCmd()
        {
            UsuarioIds = new List<string> { IdentityDatabase.AlexiaUserId }
        });
        Assert.Equal((int)System.Net.HttpStatusCode.OK, (actionResult.Result as OkObjectResult)?.StatusCode);

        grupo = await Grupos.FindAsync(x => x.Id == grupo.Id).FirstOrDefaultAsync();
        Assert.NotNull(grupo);
        subgrupo = grupo.SubGrupos.First(x => x.SubGrupoId == subgrupoId);
        Assert.Empty(subgrupo.UsuarioIds);
    }

    private async Task<Grupo> CriarGrupoSubGrupos()
    {
        var controller = CreateController<GrupoController>(Users.Samantha);
        var actionResult = await controller.Criar(new Pulsar.Services.Identity.Contracts.Commands.Grupos.CriarGrupoCmd()
        {
            Nome = "GRUPO 1"
        });
        Assert.Equal((int)System.Net.HttpStatusCode.OK, (actionResult.Result as OkObjectResult)?.StatusCode);
        var novoId = (actionResult.Result as OkObjectResult)?.Value as CreatedCommandResult;
        Assert.NotNull(novoId);

        actionResult = await controller.CriarSubgrupo(novoId.Id, new Pulsar.Services.Identity.Contracts.Commands.Grupos.CriarSubGrupoCmd()
        {
            Nome = "SUB GRUPO 1",
            PermissoesDominio = new List<PermissoesDominio> { PermissoesDominio.ConvidarUsuario },
            PermissoesEstabelecimento = new List<Pulsar.Services.Identity.Contracts.Commands.Grupos.CriarSubGrupoCmd.PermissoesEstabelecimentoOuRede>
            {
                new Pulsar.Services.Identity.Contracts.Commands.Grupos.CriarSubGrupoCmd.PermissoesEstabelecimentoOuRede()
                {
                    EstabelecimentoId = IdentityDatabase.EstabelecimentoPadraoId,
                    Permissoes = new List<PermissoesEstabelecimento> { PermissoesEstabelecimento.EditarAgendas }
                },
                new Pulsar.Services.Identity.Contracts.Commands.Grupos.CriarSubGrupoCmd.PermissoesEstabelecimentoOuRede()
                {
                    RedeEstabelecimentosId = IdentityDatabase.RedeEstabelecimentosPadraoId,
                    Permissoes = new List<PermissoesEstabelecimento> { PermissoesEstabelecimento.EditarEstabelecimento }
                }
            }
        });
        Assert.Equal((int)System.Net.HttpStatusCode.OK, (actionResult.Result as OkObjectResult)?.StatusCode);

        return await Grupos.FindAsync(x => x.Id == ObjectId.Parse(novoId.Id)).FirstOrDefaultAsync() ?? throw new InvalidOperationException("grupo não criado");
    }
}
