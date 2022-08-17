using Pulsar.BuildingBlocks.DDD.Abstractions;
using Pulsar.Services.Identity.Domain.Events;

namespace Pulsar.Services.Identity.API.Application.DomainEvents;

public class EnviarEmailRedefinicaoSenhaDomainEventHandler : IdentityDomainEventHandler<TokenMudancaSenhaGeradoDomainEvent>
{
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;

    public EnviarEmailRedefinicaoSenhaDomainEventHandler(IEmailService emailService, IConfiguration configuration, 
        IDbSession session, IEnumerable<IIsRepository> repositories) : base(session, repositories)
    {
        _emailService = emailService;
        _configuration = configuration;
    }

    protected override async Task HandleAsync(TokenMudancaSenhaGeradoDomainEvent evt, CancellationToken ct)
    {
        var url = _configuration["ResetPasswordUrl"].Replace("{TOKEN}", evt.Token).Replace("{USERID}", evt.UsuarioId);
        await _emailService.Send(new ResetPasswordViewModel(evt.Email, evt.Nome, url));
    }
}
