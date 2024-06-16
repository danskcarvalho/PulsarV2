using Bunit;
using Microsoft.AspNetCore.Components;

namespace Identity.UI.UnitTests.Pages;

public class LogoutTests : TestContext
{
    [Fact]
    public void Usuario_Deslogou_sem_Confirmacao()
    {
        string logoutId = "123456";
        //Set Up
        var mock = new Mock<ILogoutClient>();
        mock.Setup(c => c.TryLogout(It.IsAny<string>(), default))
            .Returns(Task.FromResult(new LogoutResultDTO()
            {
                LogoutId = logoutId,
                ShowSignoutPrompt = false,
                SignOutIFrameUrl = null,
                PostLogoutRedirectUri = null
            }));

        var client = mock.Object;
        Services.AddSingleton(client);
        var navigationManager = Services.GetRequiredService<NavigationManager>();
        var uri = navigationManager.GetUriWithQueryParameters(new Dictionary<string, object?>
        {
            ["LogoutId"] = logoutId
        });
        navigationManager.NavigateTo(uri);
        var cut = RenderComponent<Logout>();

        //Verify
        cut.WaitForElement(".tc-success", TimeSpan.FromSeconds(5));
        mock.Verify(c => c.TryLogout(logoutId, default), Times.Once());
    }

    [Fact]
    public void Usuario_Deslogou_com_Redirecionamento()
    {
        string logoutId = "123456";
        //Set Up
        var mock = new Mock<ILogoutClient>();
        mock.Setup(c => c.TryLogout(It.IsAny<string>(), default))
            .Returns(Task.FromResult(new LogoutResultDTO()
            {
                LogoutId = logoutId,
                ShowSignoutPrompt = false,
                SignOutIFrameUrl = null,
                PostLogoutRedirectUri = "http://www.google.com"
            }));

        var client = mock.Object;
        Services.AddSingleton(client);
        var navigationManager = Services.GetRequiredService<NavigationManager>();
        var uri = navigationManager.GetUriWithQueryParameters(new Dictionary<string, object?>
        {
            ["LogoutId"] = logoutId
        });
        navigationManager.NavigateTo(uri);
        var cut = RenderComponent<Logout>();

        //Verify
        var navManager = Services.GetRequiredService<FakeNavigationManager>();
        mock.Verify(c => c.TryLogout(logoutId, default), Times.Once());
        Assert.Equal("http://www.google.com", navManager.Uri);
    }

    [Fact]
    public void Usuario_Deslogou_com_Prompt()
    {
        string logoutId = "123456";
        //Set Up
        var mock = new Mock<ILogoutClient>();
        mock.Setup(c => c.TryLogout(It.IsAny<string>(), default))
            .Returns(Task.FromResult(new LogoutResultDTO()
            {
                LogoutId = logoutId,
                ShowSignoutPrompt = true,
                SignOutIFrameUrl = null,
                PostLogoutRedirectUri = null
            }));
        mock.Setup(c => c.Logout(It.IsAny<string>(), default))
           .Returns(Task.FromResult(new LogoutResultDTO()
           {
               LogoutId = logoutId,
               ShowSignoutPrompt = true,
               SignOutIFrameUrl = null,
               PostLogoutRedirectUri = null
           }));

        var client = mock.Object;
        Services.AddSingleton(client);
        var navigationManager = Services.GetRequiredService<NavigationManager>();
        var uri = navigationManager.GetUriWithQueryParameters(new Dictionary<string, object?>
        {
            ["LogoutId"] = logoutId
        });
        navigationManager.NavigateTo(uri);
        var cut = RenderComponent<Logout>();

        //Verify
        mock.Verify(c => c.TryLogout(logoutId, default), Times.Once());
        mock.Verify(c => c.Logout(logoutId, default), Times.Never());
        cut.WaitForElement(".tc-yes", TimeSpan.FromSeconds(5));
        Assert.Empty(cut.FindAll(".tc-success"));

        //Act
        cut.Find(".tc-yes").Click();

        //Verify
        mock.Verify(c => c.Logout(logoutId, default), Times.Once());
        cut.WaitForElement(".tc-success", TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Usuario_Deslogou_com_IFrame()
    {
        string logoutId = "123456";
        //Set Up
        var mock = new Mock<ILogoutClient>();
        mock.Setup(c => c.TryLogout(It.IsAny<string>(), default))
            .Returns(Task.FromResult(new LogoutResultDTO()
            {
                LogoutId = logoutId,
                ShowSignoutPrompt = false,
                SignOutIFrameUrl = "http://www.google.com",
                PostLogoutRedirectUri = null
            }));

        var handler = JSInterop.SetupVoid("AppendLogoutIFrame", _ => true);
        handler.SetVoidResult();
        var client = mock.Object;
        Services.AddSingleton(client);
        var navigationManager = Services.GetRequiredService<NavigationManager>();
        var uri = navigationManager.GetUriWithQueryParameters(new Dictionary<string, object?>
        {
            ["LogoutId"] = logoutId
        });
        navigationManager.NavigateTo(uri);
        var cut = RenderComponent<Logout>();

        Assert.Single(handler.Invocations);
        Assert.Equal(2, handler.Invocations.First().Arguments.Count());
        Assert.Equal("http://www.google.com", handler.Invocations.First().Arguments[1]);
    }
}
