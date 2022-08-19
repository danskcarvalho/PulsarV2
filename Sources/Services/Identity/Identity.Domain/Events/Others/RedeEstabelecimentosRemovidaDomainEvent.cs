namespace Pulsar.Services.Identity.Domain.Events.Others;

public class RedeEstabelecimentosRemovidaDomainEvent : INotification
{
    public ObjectId RedeEstabelecimentosId { get; set; }

    public RedeEstabelecimentosRemovidaDomainEvent(ObjectId redeEstabelecimentosId)
    {
        RedeEstabelecimentosId = redeEstabelecimentosId;
    }
}
