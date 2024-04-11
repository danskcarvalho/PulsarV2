using Pulsar.Services.Identity.Domain.Events.Others;
using Pulsar.Services.Identity.Domain.Specifications.Others;

namespace Pulsar.Services.Identity.Functions.Application.DomainEvents.Others;

public class RemoverRedeEstabelecimentosFromEstabelecimentosDEH : IdentityDomainEventHandler<RedeEstabelecimentosRemovidaDE>
{
    public RemoverRedeEstabelecimentosFromEstabelecimentosDEH(IdentityDomainEventHandlerContext<RedeEstabelecimentosRemovidaDE> ctx) : base(ctx)
    {
    }

    protected override async Task HandleAsync(RedeEstabelecimentosRemovidaDE evt, CancellationToken ct)
    {
        var spec = new RemoveRedesFromEstabelecimentosSpec(evt.RedeEstabelecimentosId, evt.TimeStamp);
        await EstabelecimentoRepository.UpdateManyAsync(spec, ct);
    }
}
