using Pulsar.BuildingBlocks.DDD.Abstractions;
using Pulsar.Services.Identity.API.Application.BaseTypes;
using Pulsar.Services.Identity.Domain.Events.Usuarios;

namespace Pulsar.Services.Identity.API.Application.DomainEvents.Usuarios;

public class EnviarEmailRedefinicaoSenhaDEH : IdentityDomainEventHandler<TokenMudancaSenhaGeradoDE>
{
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;

    public EnviarEmailRedefinicaoSenhaDEH(IEmailService emailService, IConfiguration configuration, IdentityDomainEventHandlerContext<TokenMudancaSenhaGeradoDE> ctx) : base(ctx)
    {
        _emailService = emailService;
        _configuration = configuration;
    }

    protected override async Task HandleAsync(TokenMudancaSenhaGeradoDE evt, CancellationToken ct)
    {
        var url = _configuration["ResetPasswordUrl"]!.Replace("{TOKEN}", evt.Token).Replace("{USERID}", evt.UsuarioId.ToString());
        await _emailService.Send(new ResetPasswordViewModel(evt.Email, evt.Nome, url));
    }
}
