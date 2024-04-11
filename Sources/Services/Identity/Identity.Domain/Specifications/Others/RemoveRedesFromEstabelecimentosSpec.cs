using Pulsar.Services.Identity.Domain.Aggregates.Others;

namespace Pulsar.Services.Identity.Domain.Specifications.Others;

public class RemoveRedesFromEstabelecimentosSpec : IUpdateSpecification<Estabelecimento>
{
    public RemoveRedesFromEstabelecimentosSpec(ObjectId redeEstabelecimentosIds, DateTime timeStamp)
    {
        RedeEstabelecimentosIds = redeEstabelecimentosIds;
        TimeStamp = timeStamp;
    }

    public ObjectId RedeEstabelecimentosIds { get; }
    public DateTime TimeStamp { get; }

    public UpdateSpecification<Estabelecimento> GetSpec()
    {
        return Update
            .Where<Estabelecimento>(x => x.Redes.Contains(RedeEstabelecimentosIds) && x.TimeStamp <= TimeStamp)
            .Pull(x => x.Redes, RedeEstabelecimentosIds)
            .Set(x => x.TimeStamp, TimeStamp)
            .Build();
    }
}
