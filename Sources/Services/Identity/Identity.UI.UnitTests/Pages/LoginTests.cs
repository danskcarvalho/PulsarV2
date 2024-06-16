using Microsoft.AspNetCore.Components;
using Pulsar.Services.Identity.Contracts.Commands.Convites;
using Pulsar.Services.Identity.Contracts.Enumerations;
using Pulsar.Services.Identity.UI.Shared;

namespace Identity.UI.UnitTests.Pages;

public class LoginTests : TestContext
{
    [Fact]
    public void Super_Administrador_Login()
    {
        //Set Up
        var (cut, mock) = SetUp(Super_Usuario);

        //Act
        cut.Find(".tc-username").Change("usuario");
        cut.Find(".tc-password").Change("$%#123Usuario%$#Usuario");
        cut.Find(".tc-continue").Click();

        //Verify
        mock.Verify(x => x.TestCredentials(It.Is<UsuarioSenhaDTO>(x => x.UsernameOrEmail == "usuario" && x.Senha == "$%#123Usuario%$#Usuario"), default));
        mock.Verify(x => x.Login(It.Is<LoginDTO>(x => x.UsernameOrEmail == "usuario" && x.Senha == "$%#123Usuario%$#Usuario"
          && x.DominioId == null && x.EstabelecimentoId == null), default), Times.Once());
        var navManager = Services.GetRequiredService<FakeNavigationManager>();
        cut.WaitForState(() => navManager.Uri == "https://www.google.com", TimeSpan.FromSeconds(5));
        Assert.Equal("https://www.google.com", navManager.Uri);
    }

    [Fact]
    public void Usuario_Inativo_Login()
    {
        //Set Up
        var (cut, mock) = SetUp(Usuario_Inativo);

        //Act
        cut.Find(".tc-username").Change("usuario");
        cut.Find(".tc-password").Change("$%#123Usuario%$#Usuario");
        cut.Find(".tc-continue").Click();

        //Verify
        mock.Verify(x => x.TestCredentials(It.Is<UsuarioSenhaDTO>(x => x.UsernameOrEmail == "usuario" && x.Senha == "$%#123Usuario%$#Usuario"), default));
        cut.Find(".tc-errormessage").TextContent.MarkupMatches("O usuário informado está bloqueado.");
    }

    [Fact]
    public void Usuario_Login_Senha_Incorreto()
    {
        //Set Up
        var (cut, mock) = SetUp(null);

        //Act
        cut.Find(".tc-username").Change("usuario");
        cut.Find(".tc-password").Change("$%#123Usuario%$#Usuario");
        cut.Find(".tc-continue").Click();

        //Verify
        mock.Verify(x => x.TestCredentials(It.Is<UsuarioSenhaDTO>(x => x.UsernameOrEmail == "usuario" && x.Senha == "$%#123Usuario%$#Usuario"), default));
        cut.Find(".tc-errormessage").TextContent.MarkupMatches("O usuário ou senha informados estão incorretos.");
    }

    [Fact]
    public void Usuario_Dominio_Login()
    {
        //Set Up
        var (cut, mock) = SetUp(Usuario_Dominio);

        //Act
        cut.Find(".tc-username").Change("usuario");
        cut.Find(".tc-password").Change("$%#123Usuario%$#Usuario");
        cut.Find(".tc-continue").Click();

        //Verify
        mock.Verify(x => x.TestCredentials(It.Is<UsuarioSenhaDTO>(x => x.UsernameOrEmail == "usuario" && x.Senha == "$%#123Usuario%$#Usuario"), default));
        mock.Verify(x => x.Login(It.Is<LoginDTO>(x => x.UsernameOrEmail == "usuario" && x.Senha == "$%#123Usuario%$#Usuario"
          && x.DominioId == "D1" && x.EstabelecimentoId == null), default), Times.Once());
        var navManager = Services.GetRequiredService<FakeNavigationManager>();
        cut.WaitForState(() => navManager.Uri == "https://www.google.com", TimeSpan.FromSeconds(5));
        Assert.Equal("https://www.google.com", navManager.Uri);
    }

    [Fact]
    public void Usuario_Administrador_Dominio_e_Multiplos_Estabelecimentos_Login()
    {
        //Set Up
        var (cut, mock) = SetUp(Usuario_Administrador_Dominio_e_Multiplos_Estabelecimentos);

        //Act
        cut.Find(".tc-username").Change("usuario");
        cut.Find(".tc-password").Change("$%#123Usuario%$#Usuario");
        cut.Find(".tc-continue").Click();

        //Verify
        mock.Verify(x => x.TestCredentials(It.Is<UsuarioSenhaDTO>(x => x.UsernameOrEmail == "usuario" && x.Senha == "$%#123Usuario%$#Usuario"), default));
        cut.WaitForElement(".tc-rad-dominio-admin", TimeSpan.FromSeconds(5));
        Assert.True(!cut.Find(".tc-rad-dominio-admin").HasAttribute("disabled"));

        //Act
        cut.Find(".tc-rad-estabelecimento").Change(new ChangeEventArgs());
        var estabelecimento = cut.FindComponent<SearchableSelect>();
        Assert.NotNull(estabelecimento);
        estabelecimento.SetParametersAndRender(parameters =>
          parameters.Add(p => p.Value, "E2"));
        cut.WaitForState(() => cut.Instance.EstabelecimentoId != null, TimeSpan.FromSeconds(5));
        cut.Find(".tc-login-establecimento").Click();

        //Verify
        mock.Verify(x => x.Login(It.Is<LoginDTO>(x => x.UsernameOrEmail == "usuario" && x.Senha == "$%#123Usuario%$#Usuario"
          && x.EstabelecimentoId == "E2"), default), Times.Once());
        var navManager = Services.GetRequiredService<FakeNavigationManager>();
        cut.WaitForState(() => navManager.Uri == "https://www.google.com", TimeSpan.FromSeconds(5));
        Assert.Equal("https://www.google.com", navManager.Uri);
    }

    [Fact]
    public void Usuario_Estabelecimento_Login()
    {
        //Set Up
        var (cut, mock) = SetUp(Usuario_Estabelecimento);

        //Act
        cut.Find(".tc-username").Change("usuario");
        cut.Find(".tc-password").Change("$%#123Usuario%$#Usuario");
        cut.Find(".tc-continue").Click();

        //Verify
        mock.Verify(x => x.TestCredentials(It.Is<UsuarioSenhaDTO>(x => x.UsernameOrEmail == "usuario" && x.Senha == "$%#123Usuario%$#Usuario"), default));
        mock.Verify(x => x.Login(It.Is<LoginDTO>(x => x.UsernameOrEmail == "usuario" && x.Senha == "$%#123Usuario%$#Usuario"
          && x.EstabelecimentoId == "E1"), default), Times.Once());
        var navManager = Services.GetRequiredService<FakeNavigationManager>();
        cut.WaitForState(() => navManager.Uri == "https://www.google.com", TimeSpan.FromSeconds(5));
        Assert.Equal("https://www.google.com", navManager.Uri);
    }

    [Fact]
    public void Usuario_Multiplos_Dominios_Login()
    {
        //Set Up
        var (cut, mock) = SetUp(Usuario_Multiplos_Dominios);

        //Act
        cut.Find(".tc-username").Change("usuario");
        cut.Find(".tc-password").Change("$%#123Usuario%$#Usuario");
        cut.Find(".tc-continue").Click();

        //Verify
        mock.Verify(x => x.TestCredentials(It.Is<UsuarioSenhaDTO>(x => x.UsernameOrEmail == "usuario" && x.Senha == "$%#123Usuario%$#Usuario"), default));
        cut.WaitForElement(".tc-select-dominio", TimeSpan.FromSeconds(5));

        //Act
        var dominio = cut.FindComponent<SearchableSelect>();
        Assert.NotNull(dominio);
        dominio.SetParametersAndRender(parameters =>
          parameters.Add(p => p.Value, "D2"));
        cut.WaitForState(() => cut.Instance.DominioId != null, TimeSpan.FromSeconds(5));
        cut.Find(".tc-login-dominio").Click();

        //Verify
        mock.Verify(x => x.Login(It.Is<LoginDTO>(x => x.UsernameOrEmail == "usuario" && x.Senha == "$%#123Usuario%$#Usuario"
          && x.DominioId == "D2"), default), Times.Once());
        var navManager = Services.GetRequiredService<FakeNavigationManager>();
        cut.WaitForState(() => navManager.Uri == "https://www.google.com", TimeSpan.FromSeconds(5));
        Assert.Equal("https://www.google.com", navManager.Uri);
    }

    [Fact]
    public void Usuario_Multiplos_Estabelecimentos_Login()
    {
        //Set Up
        var (cut, mock) = SetUp(Usuario_Multiplos_Estabelecimentos);

        //Act
        cut.Find(".tc-username").Change("usuario");
        cut.Find(".tc-password").Change("$%#123Usuario%$#Usuario");
        cut.Find(".tc-continue").Click();

        //Verify
        mock.Verify(x => x.TestCredentials(It.Is<UsuarioSenhaDTO>(x => x.UsernameOrEmail == "usuario" && x.Senha == "$%#123Usuario%$#Usuario"), default));
        cut.WaitForElement(".tc-rad-dominio-admin", TimeSpan.FromSeconds(5));
        Assert.True(cut.Find(".tc-rad-dominio-admin").HasAttribute("disabled"));

        //Act
        var estabelecimento = cut.FindComponent<SearchableSelect>();
        Assert.NotNull(estabelecimento);
        estabelecimento.SetParametersAndRender(parameters =>
          parameters.Add(p => p.Value, "E2"));
        cut.WaitForState(() => cut.Instance.EstabelecimentoId != null, TimeSpan.FromSeconds(5));
        cut.Find(".tc-login-establecimento").Click();

        //Verify
        mock.Verify(x => x.Login(It.Is<LoginDTO>(x => x.UsernameOrEmail == "usuario" && x.Senha == "$%#123Usuario%$#Usuario"
          && x.EstabelecimentoId == "E2"), default), Times.Once());
        var navManager = Services.GetRequiredService<FakeNavigationManager>();
        cut.WaitForState(() => navManager.Uri == "https://www.google.com", TimeSpan.FromSeconds(5));
        Assert.Equal("https://www.google.com", navManager.Uri);
    }

    [Fact]
    public void Usuario_Multiplos_Dominios_e_Estabelecimentos_Login()
    {
        //Set Up
        var (cut, mock) = SetUp(Usuario_Multiplos_Dominios_e_Estabelecimentos);

        //Act
        cut.Find(".tc-username").Change("usuario");
        cut.Find(".tc-password").Change("$%#123Usuario%$#Usuario");
        cut.Find(".tc-continue").Click();

        //Verify
        mock.Verify(x => x.TestCredentials(It.Is<UsuarioSenhaDTO>(x => x.UsernameOrEmail == "usuario" && x.Senha == "$%#123Usuario%$#Usuario"), default));
        cut.WaitForElement(".tc-select-dominio", TimeSpan.FromSeconds(5));

        //Act
        var dominio = cut.FindComponent<SearchableSelect>();
        Assert.NotNull(dominio);
        dominio.SetParametersAndRender(parameters =>
          parameters.Add(p => p.Value, "D2"));
        cut.WaitForState(() => cut.Instance.DominioId != null, TimeSpan.FromSeconds(5));
        cut.Find(".tc-login-dominio").Click();

        //Verify
        cut.WaitForElement(".tc-rad-dominio-admin", TimeSpan.FromSeconds(5));
        Assert.True(!cut.Find(".tc-rad-dominio-admin").HasAttribute("disabled"));

        //Act
        cut.Find(".tc-rad-estabelecimento").Change(new ChangeEventArgs());
        var estabelecimento = cut.FindComponent<SearchableSelect>();
        Assert.NotNull(estabelecimento);
        estabelecimento.SetParametersAndRender(parameters =>
          parameters.Add(p => p.Value, "E4"));
        cut.WaitForState(() => cut.Instance.EstabelecimentoId != null, TimeSpan.FromSeconds(5));
        cut.Find(".tc-login-establecimento").Click();

        //Verify
        mock.Verify(x => x.Login(It.Is<LoginDTO>(x => x.UsernameOrEmail == "usuario" && x.Senha == "$%#123Usuario%$#Usuario"
          && x.EstabelecimentoId == "E4"), default), Times.Once());
        var navManager = Services.GetRequiredService<FakeNavigationManager>();
        cut.WaitForState(() => navManager.Uri == "https://www.google.com", TimeSpan.FromSeconds(5));
        Assert.Equal("https://www.google.com", navManager.Uri);
    }

    private (IRenderedComponent<Login>, Mock<ILoginClient>) SetUp(UsuarioLogadoDTO? usuarioLogado)
    {
        var mockLogin = new Mock<ILoginClient>();
        mockLogin.Setup(x => x.TestCredentials(It.IsAny<UsuarioSenhaDTO>(), default)).Returns(Task.FromResult(usuarioLogado));
        mockLogin.Setup(x => x.Login(It.IsAny<LoginDTO>(), default)).Returns(Task.FromResult((LoginResultDTO?)new LoginResultDTO()
        {
            Ok = true,
            RedirectUrl = "https://www.google.com"
        }));
        var client = mockLogin.Object;


        JSInterop.SetupVoid("InitializeSelectComponents", _ => true).SetVoidResult();
        Services.AddSingleton(client);

        var navigationManager = Services.GetRequiredService<NavigationManager>();
        var uri = navigationManager.GetUriWithQueryParameters(new Dictionary<string, object?>
        {
            ["ReturnUrl"] = "https://www.microsoft.com"
        });
        navigationManager.NavigateTo(uri);
        var cut = RenderComponent<Login>();
        return (cut, mockLogin);
    }

    private static readonly UsuarioLogadoDTO Super_Usuario = new UsuarioLogadoDTO("1", "Usuario", null, "Usuario", null,
      "usuario", true, true, null, new List<UsuarioLogadoDTO.DominioDTO>());
    private static readonly UsuarioLogadoDTO Usuario_Inativo = new UsuarioLogadoDTO("1", "Usuario", null, "Usuario", null,
      "usuario", false, false, null, new List<UsuarioLogadoDTO.DominioDTO>());
    private static readonly UsuarioLogadoDTO Usuario_Dominio = new UsuarioLogadoDTO("1", "Usuario", null, "Usuario", null,
      "usuario", true, false, null, new List<UsuarioLogadoDTO.DominioDTO>()
      {
          new UsuarioLogadoDTO.DominioDTO("D1", "D1", false, true, Enum.GetValues<PermissoesDominio>().ToList(), new List<UsuarioLogadoDTO.EstabelecimentoDTO>())
      });
    private static readonly UsuarioLogadoDTO Usuario_Administrador_Dominio_e_Multiplos_Estabelecimentos = new UsuarioLogadoDTO("1", "Usuario", null, "Usuario", null,
      "usuario", true, false, null, new List<UsuarioLogadoDTO.DominioDTO>()
      {
      new UsuarioLogadoDTO.DominioDTO("D1", "D1", true, true, Enum.GetValues<PermissoesDominio>().ToList(),
        new List<UsuarioLogadoDTO.EstabelecimentoDTO>()
        {
            new UsuarioLogadoDTO.EstabelecimentoDTO("E1", "E1", Enum.GetValues<PermissoesEstabelecimento>().ToList()),
            new UsuarioLogadoDTO.EstabelecimentoDTO("E2", "E2", Enum.GetValues<PermissoesEstabelecimento>().ToList())
        })
      });
    private static readonly UsuarioLogadoDTO Usuario_Estabelecimento = new UsuarioLogadoDTO("1", "Usuario", null, "Usuario", null,
      "usuario", true, false, null, new List<UsuarioLogadoDTO.DominioDTO>()
      {
          new UsuarioLogadoDTO.DominioDTO("D1", "D1", false, false, Enum.GetValues<PermissoesDominio>().ToList(),
              new List<UsuarioLogadoDTO.EstabelecimentoDTO>()
              {
                  new UsuarioLogadoDTO.EstabelecimentoDTO("E1", "E1", Enum.GetValues<PermissoesEstabelecimento>().ToList())
              })
      });
    private static readonly UsuarioLogadoDTO Usuario_Multiplos_Dominios = new UsuarioLogadoDTO("1", "Usuario", null, "Usuario", null,
      "usuario", true, false, null, new List<UsuarioLogadoDTO.DominioDTO>()
      {
          new UsuarioLogadoDTO.DominioDTO("D1", "D1", false, true, Enum.GetValues<PermissoesDominio>().ToList(), new List<UsuarioLogadoDTO.EstabelecimentoDTO>()),
          new UsuarioLogadoDTO.DominioDTO("D2", "D2", false, true, Enum.GetValues<PermissoesDominio>().ToList(), new List<UsuarioLogadoDTO.EstabelecimentoDTO>())
      });
    private static readonly UsuarioLogadoDTO Usuario_Multiplos_Estabelecimentos = new UsuarioLogadoDTO("1", "Usuario", null, "Usuario", null,
      "usuario", true, false, null, new List<UsuarioLogadoDTO.DominioDTO>()
      {
          new UsuarioLogadoDTO.DominioDTO("D1", "D1", false, false, Enum.GetValues<PermissoesDominio>().ToList(),
              new List<UsuarioLogadoDTO.EstabelecimentoDTO>()
              {
                  new UsuarioLogadoDTO.EstabelecimentoDTO("E1", "E1", Enum.GetValues<PermissoesEstabelecimento>().ToList()),
                  new UsuarioLogadoDTO.EstabelecimentoDTO("E2", "E2", Enum.GetValues<PermissoesEstabelecimento>().ToList())
              })
      });
    private static readonly UsuarioLogadoDTO Usuario_Multiplos_Dominios_e_Estabelecimentos = new UsuarioLogadoDTO("1", "Usuario", null, "Usuario", null,
      "usuario", true, false, null, new List<UsuarioLogadoDTO.DominioDTO>()
      {
          new UsuarioLogadoDTO.DominioDTO("D1", "D1", false, true, Enum.GetValues<PermissoesDominio>().ToList(),
              new List<UsuarioLogadoDTO.EstabelecimentoDTO>()
              {
                  new UsuarioLogadoDTO.EstabelecimentoDTO("E1", "E1", Enum.GetValues<PermissoesEstabelecimento>().ToList()),
                  new UsuarioLogadoDTO.EstabelecimentoDTO("E2", "E2", Enum.GetValues<PermissoesEstabelecimento>().ToList())
              }),
          new UsuarioLogadoDTO.DominioDTO("D2", "D2", false, true, Enum.GetValues<PermissoesDominio>().ToList(),
              new List<UsuarioLogadoDTO.EstabelecimentoDTO>()
              {
                  new UsuarioLogadoDTO.EstabelecimentoDTO("E3", "E3", Enum.GetValues<PermissoesEstabelecimento>().ToList()),
                  new UsuarioLogadoDTO.EstabelecimentoDTO("E4", "E4", Enum.GetValues<PermissoesEstabelecimento>().ToList())
              })
      });
}
