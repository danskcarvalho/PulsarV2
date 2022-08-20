using Pulsar.BuildingBlocks.Emails.Abstractions;
using Pulsar.Services.Identity.API.Application.BaseTypes;
using Pulsar.Services.Identity.Domain.Events.Convites;

namespace Pulsar.Services.Identity.API.Application.DomainEvents.Usuarios;

public class EnviarEmailUsuarioConvidadoDomainEventHandler : IdentityDomainEventHandler<UsuarioConvidadoDomainEvent>
{
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;

    public EnviarEmailUsuarioConvidadoDomainEventHandler(IEmailService emailService, IConfiguration configuration, 
        ILogger<IdentityDomainEventHandler<UsuarioConvidadoDomainEvent>> logger, IDbSession session, IEnumerable<IIsRepository> repositories) : base(logger, session, repositories)
    {
        _emailService = emailService;
        _configuration = configuration;
    }

    protected override Task HandleAsync(UsuarioConvidadoDomainEvent evt, CancellationToken ct)
    {
        EnviarEmail(evt);
        return Task.CompletedTask;
    }

    private async void EnviarEmail(UsuarioConvidadoDomainEvent evt)
    {
        var url = _configuration["AceitarConviteUrl"].Replace("{TOKEN}", evt.TokenAceitacao).Replace("{CONVITEID}", evt.ConviteId.ToString());
        await _emailService.Send(new ConvidarUsuarioViewModel(evt.Email, evt.Email, url));
    }
}
