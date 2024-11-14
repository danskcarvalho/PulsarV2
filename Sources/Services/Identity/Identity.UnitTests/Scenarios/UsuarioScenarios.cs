using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using Pulsar.BuildingBlocks.UnitTests.Mocking.FileSystem;

namespace Identity.UnitTests.Scenarios;

public class UsuarioScenarios : IdentityScenarios
{
    [Fact]
    public async Task Bloquear_e_Desbloquear_Usuario()
    {
        var alexiaId = ObjectId.Parse(IdentityDatabase.AlexiaUserId);
        var alexia = await Usuarios.FindAsync(x => x.Id == alexiaId).FirstOrDefaultAsync();
        Assert.NotNull(alexia);
        Assert.True(alexia.IsAtivo);

        var controller = CreateController<UsuarioController>(Users.Admin);
        var actionResult = await controller.Bloquear(IdentityDatabase.AlexiaUserId);
        Assert.Equal((int)System.Net.HttpStatusCode.OK, (actionResult.Result as OkObjectResult)?.StatusCode);

        alexia = await Usuarios.FindAsync(x => x.Id == alexiaId).FirstOrDefaultAsync();
        Assert.NotNull(alexia);
        Assert.False(alexia.IsAtivo);

        actionResult = await controller.Desbloquear(IdentityDatabase.AlexiaUserId);
        Assert.Equal((int)System.Net.HttpStatusCode.OK, (actionResult.Result as OkObjectResult)?.StatusCode);

        alexia = await Usuarios.FindAsync(x => x.Id == alexiaId).FirstOrDefaultAsync();
        Assert.NotNull(alexia);
        Assert.True(alexia.IsAtivo);
    }

    [Fact]
    public async Task Editar_Meus_Dados()
    {
        var controller = CreateController<UsuarioController>(Users.Alexia);
        var actionResult = await controller.EditarMeusDados(new Pulsar.Services.Identity.Contracts.Commands.Usuarios.EditarMeusDadosCmd()
        {
            PrimeiroNome = "Alexandria",
            Sobrenome = "Maia"
        });
        Assert.Equal((int)System.Net.HttpStatusCode.OK, (actionResult.Result as OkObjectResult)?.StatusCode);

        var alexiaId = ObjectId.Parse(IdentityDatabase.AlexiaUserId);
        var alexia = await Usuarios.FindAsync(x => x.Id == alexiaId).FirstOrDefaultAsync();
        Assert.NotNull(alexia);
        Assert.Equal("Alexandria", alexia.PrimeiroNome);
        Assert.Equal("Maia", alexia.UltimoNome);
    }

    [Fact]
    public async Task Mudar_Minha_Senha()
    {
        var controller = CreateController<UsuarioController>(Users.Alexia);
        var exception = await Assert.ThrowsAsync<IdentityDomainException>(async () => await controller.MudarMinhaSenha(new Pulsar.Services.Identity.Contracts.Commands.Usuarios.MudarMinhaSenhaCmd()
        {
            SenhaAtual = "jsdfikhhdf8934",
            Senha = "$%bYbAw9gYCxKfbkxF",
            ConfirmarSenha = "$%bYbAw9gYCxKfbkxF"
        }));
        Assert.Equal(ExceptionKey.SenhaAtualInvalida, exception.Key);

        var actionResult = await controller.MudarMinhaSenha(new Pulsar.Services.Identity.Contracts.Commands.Usuarios.MudarMinhaSenhaCmd()
        {
            SenhaAtual = "123456",
            Senha = "$%bYbAw9gYCxKfbkxF",
            ConfirmarSenha = "$%bYbAw9gYCxKfbkxF"
        });
        Assert.Equal((int)System.Net.HttpStatusCode.OK, (actionResult.Result as OkObjectResult)?.StatusCode);

        var alexiaId = ObjectId.Parse(IdentityDatabase.AlexiaUserId);
        var alexia = await Usuarios.FindAsync(x => x.Id == alexiaId).FirstOrDefaultAsync();
        Assert.NotNull(alexia);
        Assert.Equal((alexia.SenhaSalt + "$%bYbAw9gYCxKfbkxF").SHA256Hash(), alexia.SenhaHash);
    }

    [Fact]
    public async Task Mudar_Meu_Avatar()
    {
        var alexiaId = ObjectId.Parse(IdentityDatabase.AlexiaUserId);
        var alexia = await Usuarios.FindAsync(x => x.Id == alexiaId).FirstOrDefaultAsync();
        Assert.NotNull(alexia);
        Assert.Null(alexia.AvatarUrl);

        var controller = CreateController<UsuarioController>(Users.Alexia);
        var imageStream = new FileStream("Images/Face.jpg", FileMode.Open);
        var formFile = new FormFile(imageStream, 0, imageStream.Length, "Imagem", "Face.jpg");
        formFile.Headers = new HeaderDictionary();
        formFile.ContentType = "image/jpeg";
		var actionResult = await controller.MudarMeuAvatar(new Pulsar.Services.Identity.API.ViewModels.MudarMeuAvatarViewModel()
        {
            Imagem = formFile
        });
        Assert.Equal((int)System.Net.HttpStatusCode.OK, (actionResult.Result as OkObjectResult)?.StatusCode);
        alexia = await Usuarios.FindAsync(x => x.Id == alexiaId).FirstOrDefaultAsync();
        Assert.NotNull(alexia);
        Assert.NotNull(alexia.AvatarUrl);

        var fs = Services.GetRequiredService<MockedFileSystem>();
        Assert.Single(fs.FilesUploaded);
    }
}
