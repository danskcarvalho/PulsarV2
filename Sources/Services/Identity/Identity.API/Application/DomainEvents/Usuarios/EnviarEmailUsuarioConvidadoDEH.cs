using Pulsar.BuildingBlocks.Emails.Abstractions;
using Pulsar.Services.Identity.API.Application.BaseTypes;
using Pulsar.Services.Identity.Domain.Events.Convites;

namespace Pulsar.Services.Identity.API.Application.DomainEvents.Usuarios;

public class EnviarEmailUsuarioConvidadoDEH : IdentityDomainEventHandler<UsuarioConvidadoDE>
{
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;

    public EnviarEmailUsuarioConvidadoDEH(IEmailService emailService, IConfiguration configuration, IdentityDomainEventHandlerContext<UsuarioConvidadoDE> ctx) : base(ctx)
    {
        _emailService = emailService;
        _configuration = configuration;
    }

    protected override Task HandleAsync(UsuarioConvidadoDE evt, CancellationToken ct)
    {
        EnviarEmail(evt);
        return Task.CompletedTask;
    }

    private async void EnviarEmail(UsuarioConvidadoDE evt)
    {
        var url = _configuration["AceitarConviteUrl"]!.Replace("{TOKEN}", evt.TokenAceitacao).Replace("{CONVITEID}", evt.ConviteId.ToString());
        await _emailService.Send(new ConvidarUsuarioViewModel(evt.Email, evt.Email, url));
    }
}
