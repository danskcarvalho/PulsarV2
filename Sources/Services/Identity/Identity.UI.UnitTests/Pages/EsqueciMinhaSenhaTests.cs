namespace Identity.UI.UnitTests.Pages;

public class EsqueciMinhaSenhaTests : TestContext
{
  [Fact]
  public void Esqueci_Minha_Senha()
  {
    EsqueciMinhaSenhaCmd? cmd = null;
    //Set Up
    var mock = new Mock<IEsqueciMinhaSenhaClient>();
    mock.Setup(c => c.EsqueciMinhaSenha(It.IsAny<EsqueciMinhaSenhaCmd>(), default))
        .Callback((EsqueciMinhaSenhaCmd sent, CancellationToken ct) =>
        {
          cmd = sent;
        })
        .Returns(Task.CompletedTask);

    var client = mock.Object;
    Services.AddSingleton(client);
    var cut = RenderComponent<EsqueciMinhaSenha>(parameters =>
      parameters
        .Add(p => p.ReturnUrl, "https://google.com"));

    //Act
    cut.Find(".tc-username").Change("daniwind");
    cut.Find(".tc-continue").Click();

    //Verify
    Assert.NotNull(cmd);
    mock.Verify(c => c.EsqueciMinhaSenha(It.IsAny<EsqueciMinhaSenhaCmd>(), default), Times.Once());
    Assert.Equal("daniwind", cmd.UsernameOrEmail);
    Assert.Single(cut.FindAll(".tc-success"));
    Assert.Contains("daniwind", cut.Find(".tc-success").TextContent);

    //Act (pt. 2)
    cut.Find(".tc-resend").Click();

    //Verify (pt. 2)
    mock.Verify(c => c.EsqueciMinhaSenha(It.IsAny<EsqueciMinhaSenhaCmd>(), default), Times.Exactly(2));
    Assert.NotNull(cmd);
    Assert.Equal("daniwind", cmd.UsernameOrEmail);
  }
}
