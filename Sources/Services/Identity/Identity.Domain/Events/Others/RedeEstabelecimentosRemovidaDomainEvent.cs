namespace Pulsar.Services.Identity.Domain.Events.Others;

public class RedeEstabelecimentosRemovidaDomainEvent : INotification
{
    public ObjectId RedeEstabelecimentosId { get; set; }
    public DateTime TimeStamp { get; set; }

    public RedeEstabelecimentosRemovidaDomainEvent(ObjectId redeEstabelecimentosId, DateTime timeStamp)
    {
        RedeEstabelecimentosId = redeEstabelecimentosId;
        TimeStamp = timeStamp;
    }
}
