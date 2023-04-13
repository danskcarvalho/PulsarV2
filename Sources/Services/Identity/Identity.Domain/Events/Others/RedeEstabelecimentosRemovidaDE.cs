namespace Pulsar.Services.Identity.Domain.Events.Others;

public class RedeEstabelecimentosRemovidaDE : INotification
{
    public ObjectId RedeEstabelecimentosId { get; set; }
    public DateTime TimeStamp { get; set; }

    public RedeEstabelecimentosRemovidaDE(ObjectId redeEstabelecimentosId, DateTime timeStamp)
    {
        RedeEstabelecimentosId = redeEstabelecimentosId;
        TimeStamp = timeStamp;
    }
}
