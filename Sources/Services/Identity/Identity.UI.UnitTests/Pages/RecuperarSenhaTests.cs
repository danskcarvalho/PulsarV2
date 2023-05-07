namespace Identity.UI.UnitTests.Pages;

public class RecuperarSenhaTests : TestContext
{
    [Fact]
    public void Usuario_Recupera_Senha()
    {
        string newPassword = "$%PwD%$123$%PwD%$124$%PwD%$";
        string? password = null;

        //Set Up
        var mock = new Mock<IEsqueciMinhaSenhaClient>();
        mock.Setup(c => c.RecuperarSenha(It.IsAny<RecuperarSenhaCmd>(), default))
            .Callback((RecuperarSenhaCmd cmd, CancellationToken ct) =>
            {
                password = cmd.Senha;
            })
            .Returns(Task.CompletedTask);

        var client = mock.Object;
        Services.AddSingleton(client);
        var cut = RenderComponent<RecuperarSenha>(parameters =>
          parameters
            .Add(p => p.Token, "0")
            .Add(p => p.UserId, "1"));

        var senha = cut.Find(".tc-senha");
        Assert.NotNull(senha);
        var confirmarSenha = cut.Find(".tc-confirmar-senha");
        Assert.NotNull(confirmarSenha);
        var continuar = cut.Find(".tc-continuar");
        Assert.NotNull(continuar);
        var senhaAlterada = cut.FindAll(".tc-senha-alterada");
        Assert.Empty(senhaAlterada);

        //Act
        senha.Change(newPassword);
        confirmarSenha.Change(newPassword);
        continuar.Click();

        //Verify
        mock.Verify(c => c.RecuperarSenha(It.IsAny<RecuperarSenhaCmd>(), default), Times.Once());
        Assert.Equal(newPassword, password);
        senhaAlterada = cut.FindAll(".tc-senha-alterada");
        Assert.Single(senhaAlterada);
    }
}
