﻿using Pulsar.Services.Identity.Domain.Events.Others;
using Pulsar.Services.Identity.Domain.Specifications;

namespace Pulsar.Services.Identity.BackgroundTasks.Application.DomainEvents.Others;

public class RemoverRedeEstabelecimentosFromEstabelecimentosDomainEventHandler : IdentityDomainEventHandler<RedeEstabelecimentosRemovidaDomainEvent>
{
    public RemoverRedeEstabelecimentosFromEstabelecimentosDomainEventHandler(ILogger<IdentityDomainEventHandler<RedeEstabelecimentosRemovidaDomainEvent>> logger, IDbSession session, IEnumerable<IIsRepository> repositories) : base(logger, session, repositories)
    {
    }

    protected override async Task HandleAsync(RedeEstabelecimentosRemovidaDomainEvent evt, CancellationToken ct)
    {
        var spec = new RemoveRedesFromEstabelecimentosSpec(evt.RedeEstabelecimentosId, evt.TimeStamp);
        await EstabelecimentoRepository.UpdateManyAsync(spec, ct);
    }
}