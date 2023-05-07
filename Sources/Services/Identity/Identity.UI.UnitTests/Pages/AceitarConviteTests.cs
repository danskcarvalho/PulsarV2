using Pulsar.Services.Identity.Contracts.Commands.Convites;

namespace Identity.UI.UnitTests.Pages;

public class AceitarConviteTests : TestContext
{
    [Fact]
    public void Usuario_Aceita_o_Convite()
    {

        string newPassword = "$%PwD%$123$%PwD%$124$%PwD%$";

        //Set Up
        AceitarConviteCmd? aceitarConviteCmd = null;
        var mock = new Mock<IAceitarConviteClient>();
        mock.Setup(x => x.Aceitar(It.IsAny<AceitarConviteCmd>(), default)).Returns(Task.CompletedTask)
            .Callback((AceitarConviteCmd cmd, CancellationToken ct) =>
            {
                aceitarConviteCmd = cmd;
            });
        var client = mock.Object;
        Services.AddSingleton(client);

        var cut = RenderComponent<AceitarConvite>(parameters =>
          parameters
            .Add(p => p.ConviteId, "0")
            .Add(p => p.Token, "1"));

        //Pre-Verify
        Assert.Empty(cut.FindAll(".tc-successo"));

        //Act
        cut.Find(".tc-primeiro-nome").Change("Danilo");
        cut.Find(".tc-sobrenome").Change("Windsor");
        cut.Find(".tc-nome-usuario").Change("daniwind");
        cut.Find(".tc-senha").Change(newPassword);
        cut.Find(".tc-confirmar-senha").Change(newPassword);
        cut.Find(".tc-continuar").Click();

        //Verify
        Assert.NotNull(aceitarConviteCmd);
        mock.Verify(c => c.Aceitar(It.IsAny<AceitarConviteCmd>(), default), Times.Once());
        Assert.Equal(newPassword, aceitarConviteCmd.Senha);
        Assert.Equal(newPassword, aceitarConviteCmd.ConfirmarSenha);
        Assert.Equal("daniwind", aceitarConviteCmd.NomeUsuario);
        Assert.Equal("Windsor", aceitarConviteCmd.Sobrenome);
        Assert.Equal("Danilo", aceitarConviteCmd.PrimeiroNome);
        Assert.Single(cut.FindAll(".tc-successo"));
    }
}
