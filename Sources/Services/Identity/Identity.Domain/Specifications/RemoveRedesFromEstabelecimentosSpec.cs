using Pulsar.Services.Identity.Domain.Aggregates.Others;

namespace Pulsar.Services.Identity.Domain.Specifications;

public class RemoveRedesFromEstabelecimentosSpec : IUpdateSpecification<Estabelecimento>
{
    public RemoveRedesFromEstabelecimentosSpec(ObjectId redeEstabelecimentoIds, DateTime timeStamp)
    {
        RedeEstabelecimentoIds = redeEstabelecimentoIds;
        TimeStamp = timeStamp;
    }

    public ObjectId RedeEstabelecimentoIds { get; }
    public DateTime TimeStamp { get; }  

    public UpdateSpecification<Estabelecimento> GetSpec()
    {
        return Update
            .Where<Estabelecimento>(x => x.Redes.Contains(RedeEstabelecimentoIds) && x.TimeStamp <= TimeStamp)
            .Pull(x => x.Redes, RedeEstabelecimentoIds)
            .Set(x => x.TimeStamp, TimeStamp)
            .Build();
    }
}
