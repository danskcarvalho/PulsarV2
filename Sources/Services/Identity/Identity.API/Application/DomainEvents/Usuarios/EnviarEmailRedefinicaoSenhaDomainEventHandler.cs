using Pulsar.BuildingBlocks.DDD.Abstractions;
using Pulsar.Services.Identity.API.Application.BaseTypes;
using Pulsar.Services.Identity.Domain.Events.Usuarios;

namespace Pulsar.Services.Identity.API.Application.DomainEvents.Usuarios;

public class EnviarEmailRedefinicaoSenhaDomainEventHandler : IdentityDomainEventHandler<TokenMudancaSenhaGeradoDomainEvent>
{
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;

    public EnviarEmailRedefinicaoSenhaDomainEventHandler(IEmailService emailService, IConfiguration configuration, 
        ILogger<IdentityDomainEventHandler<TokenMudancaSenhaGeradoDomainEvent>> logger, IDbSession session, IEnumerable<IIsRepository> repositories) : base(logger, session, repositories)
    {
        _emailService = emailService;
        _configuration = configuration;
    }

    protected override async Task HandleAsync(TokenMudancaSenhaGeradoDomainEvent evt, CancellationToken ct)
    {
        var url = _configuration["ResetPasswordUrl"].Replace("{TOKEN}", evt.Token).Replace("{USERID}", evt.UsuarioId.ToString());
        await _emailService.Send(new ResetPasswordViewModel(evt.Email, evt.Nome, url));
    }
}
