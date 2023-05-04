namespace Identity.UnitTests;

public class ConviteScenarios : IdentityScenarios
{
    [Fact]
    public async Task Convidar_Usuario()
    {
        var controller = CreateController<ConviteController>(Users.Admin);
        var actionResult = await controller.Criar(new Pulsar.Services.Identity.Contracts.Commands.Convites.CriarConviteCmd()
        {
            Email = "someuser@domain.com",
            UsuarioLogadoId = IdentityDatabase.AdminUserId
        });

        Assert.Equal((int)System.Net.HttpStatusCode.OK, (actionResult as OkResult)?.StatusCode);
        var createdInvite = await Convites.FindAsync(x => x.Email == "someuser@domain.com").FirstOrDefaultAsync();

        Assert.NotNull(createdInvite);
        Assert.NotNull(createdInvite.TokenAceitacao);
        Assert.NotNull(createdInvite.AuditInfo?.CriadoEm);
        Assert.Equal(createdInvite.AuditInfo.CriadoPorUsuarioId.ToString(), IdentityDatabase.AdminUserId);
        Assert.False(createdInvite.IsAceito);
        Assert.True(createdInvite.ConviteExpiraEm >= DateTime.UtcNow);

        var emails = Services.GetRequiredService<MockedEmailService>();
        Assert.True(emails.EmailsSent.Count == 1);
    }

    [Fact]
    public async Task Convidar_Usuario_e_Aceitar()
    {
        var controller = CreateController<ConviteController>(Users.Admin);
        var actionResult = await controller.Criar(new Pulsar.Services.Identity.Contracts.Commands.Convites.CriarConviteCmd()
        {
            Email = "someuser@domain.com",
            UsuarioLogadoId = IdentityDatabase.AdminUserId
        });
        Assert.Equal((int)System.Net.HttpStatusCode.OK, (actionResult as OkResult)?.StatusCode);

        var createdInvite = await Convites.FindAsync(x => x.Email == "someuser@domain.com").FirstOrDefaultAsync();
        Assert.NotNull(createdInvite); 
        Assert.False(createdInvite.IsAceito);

        var acceptController = CreateController<AceitarConviteController>();
        var password = "%$$bjdbguiyFGYU431";
        actionResult = await acceptController.Aceitar(new Pulsar.Services.Identity.Contracts.Commands.Convites.AceitarConviteCmd()
        {
            NomeUsuario = "someuser",
            Senha = password,
            ConfirmarSenha = password,
            ConviteId = createdInvite.Id.ToString(),
            PrimeiroNome = "Simone",
            Sobrenome = "Drummond",
            Token = createdInvite.TokenAceitacao
        });
        Assert.Equal((int)System.Net.HttpStatusCode.OK, (actionResult as OkResult)?.StatusCode);

        createdInvite = await Convites.FindAsync(x => x.Email == "someuser@domain.com").FirstOrDefaultAsync();
        Assert.NotNull(createdInvite);
        Assert.True(createdInvite.IsAceito);

        var usuarioSpec = Find.Where<Usuario>(x => x.Email == "someuser@domain.com").ToWrapper();
        var createdUser = await Usuarios.FindAsync(usuarioSpec).FirstOrDefaultAsync();
        Assert.NotNull(createdUser);
        Assert.Equal("Simone", createdUser.PrimeiroNome);
        Assert.Equal("Drummond", createdUser.UltimoNome);
        Assert.Equal((createdUser.SenhaSalt + password).SHA256Hash(), createdUser.SenhaHash);
    }

    [Fact]
    public async Task Convidar_Usuario_e_Aceitar_Duas_Vezes()
    {
        var controller = CreateController<ConviteController>(Users.Admin);
        var actionResult = await controller.Criar(new Pulsar.Services.Identity.Contracts.Commands.Convites.CriarConviteCmd()
        {
            Email = "someuser@domain.com",
            UsuarioLogadoId = IdentityDatabase.AdminUserId
        });
        Assert.Equal((int)System.Net.HttpStatusCode.OK, (actionResult as OkResult)?.StatusCode);

        var createdInvite = await Convites.FindAsync(x => x.Email == "someuser@domain.com").FirstOrDefaultAsync();
        Assert.NotNull(createdInvite);
        Assert.False(createdInvite.IsAceito);

        var acceptController = CreateController<AceitarConviteController>();
        var password = "%$$bjdbguiyFGYU431";
        actionResult = await acceptController.Aceitar(new Pulsar.Services.Identity.Contracts.Commands.Convites.AceitarConviteCmd()
        {
            NomeUsuario = "someuser",
            Senha = password,
            ConfirmarSenha = password,
            ConviteId = createdInvite.Id.ToString(),
            PrimeiroNome = "Simone",
            Sobrenome = "Drummond",
            Token = createdInvite.TokenAceitacao
        });
        Assert.Equal((int)System.Net.HttpStatusCode.OK, (actionResult as OkResult)?.StatusCode);

        createdInvite = await Convites.FindAsync(x => x.Email == "someuser@domain.com").FirstOrDefaultAsync();
        Assert.NotNull(createdInvite);
        Assert.True(createdInvite.IsAceito);

        var exception = await Assert.ThrowsAsync<IdentityDomainException>(async () => await acceptController.Aceitar(new Pulsar.Services.Identity.Contracts.Commands.Convites.AceitarConviteCmd()
        {
            NomeUsuario = "someuser",
            Senha = password,
            ConfirmarSenha = password,
            ConviteId = createdInvite.Id.ToString(),
            PrimeiroNome = "Simone",
            Sobrenome = "Drummond",
            Token = createdInvite.TokenAceitacao
        }));

        Assert.Equal(ExceptionKey.ConviteJaAceito, exception.Key);

    }

    [Fact]
    public async Task Convidar_Usuario_Duas_Vezes()
    {
        var controller = CreateController<ConviteController>(Users.Admin);
        var actionResult = await controller.Criar(new Pulsar.Services.Identity.Contracts.Commands.Convites.CriarConviteCmd()
        {
            Email = "someuser@domain.com",
            UsuarioLogadoId = IdentityDatabase.AdminUserId
        });
        Assert.Equal((int)System.Net.HttpStatusCode.OK, (actionResult as OkResult)?.StatusCode);

        var createdInvite = await Convites.FindAsync(x => x.Email == "someuser@domain.com").FirstOrDefaultAsync();
        Assert.NotNull(createdInvite);
        Assert.False(createdInvite.IsAceito);

        actionResult = await controller.Criar(new Pulsar.Services.Identity.Contracts.Commands.Convites.CriarConviteCmd()
        {
            Email = "someuser@domain.com",
            UsuarioLogadoId = IdentityDatabase.AdminUserId
        });
        Assert.Equal((int)System.Net.HttpStatusCode.OK, (actionResult as OkResult)?.StatusCode);
        var allConvites = await Convites.FindAsync(x => true).ToListAsync();
        Assert.Equal(2, allConvites.Count);
    }

    [Fact]
    public async Task Convidar_Usuario_e_Aceitar_eEntao_Convidar_deNovo()
    {
        var controller = CreateController<ConviteController>(Users.Admin);
        var actionResult = await controller.Criar(new Pulsar.Services.Identity.Contracts.Commands.Convites.CriarConviteCmd()
        {
            Email = "someuser@domain.com",
            UsuarioLogadoId = IdentityDatabase.AdminUserId
        });
        Assert.Equal((int)System.Net.HttpStatusCode.OK, (actionResult as OkResult)?.StatusCode);

        var createdInvite = await Convites.FindAsync(x => x.Email == "someuser@domain.com").FirstOrDefaultAsync();
        Assert.NotNull(createdInvite);
        Assert.False(createdInvite.IsAceito);

        var acceptController = CreateController<AceitarConviteController>();
        var password = "%$$bjdbguiyFGYU431";
        actionResult = await acceptController.Aceitar(new Pulsar.Services.Identity.Contracts.Commands.Convites.AceitarConviteCmd()
        {
            NomeUsuario = "someuser",
            Senha = password,
            ConfirmarSenha = password,
            ConviteId = createdInvite.Id.ToString(),
            PrimeiroNome = "Simone",
            Sobrenome = "Drummond",
            Token = createdInvite.TokenAceitacao
        });
        Assert.Equal((int)System.Net.HttpStatusCode.OK, (actionResult as OkResult)?.StatusCode);

        createdInvite = await Convites.FindAsync(x => x.Email == "someuser@domain.com").FirstOrDefaultAsync();
        Assert.NotNull(createdInvite);
        Assert.True(createdInvite.IsAceito);

        var exception = await Assert.ThrowsAsync<IdentityDomainException>(async () => await controller.Criar(new Pulsar.Services.Identity.Contracts.Commands.Convites.CriarConviteCmd()
        {
            Email = "someuser@domain.com",
            UsuarioLogadoId = IdentityDatabase.AdminUserId
        }));
        Assert.Equal(ExceptionKey.UsuarioJaConvidado, exception.Key);

    }
}