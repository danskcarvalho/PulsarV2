using Pulsar.Services.Identity.Domain.Aggregates.Others;

namespace Pulsar.Services.Identity.Domain.Specifications;

public class RemoveRedesFromEstabelecimentosSpec : IUpdateSpecification<Estabelecimento>
{
    public RemoveRedesFromEstabelecimentosSpec(ObjectId redeEstabelecimentoIds)
    {
        RedeEstabelecimentoIds = redeEstabelecimentoIds;
    }

    public ObjectId RedeEstabelecimentoIds { get; }
    public UpdateSpecification<Estabelecimento> GetSpec()
    {
        return Update.Where<Estabelecimento>(x => x.Redes.Contains(RedeEstabelecimentoIds)).Pull(x => x.Redes, RedeEstabelecimentoIds).Build();
    }
}
