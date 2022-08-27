using Pulsar.Services.Identity.Domain.Aggregates.Others;

namespace Pulsar.Services.Identity.Domain.Specifications;

public class FindRedesEstabelecimentosDominioSpec : IFindSpecification<RedeEstabelecimentos>
{
    public FindRedesEstabelecimentosDominioSpec(ObjectId dominioId)
    {
        DominioId = dominioId;
    }

    public ObjectId DominioId { get; }

    public FindSpecification<RedeEstabelecimentos> GetSpec()
    {
        return Find.Where<RedeEstabelecimentos>(x => x.DominioId == DominioId).Build();
    }
}
