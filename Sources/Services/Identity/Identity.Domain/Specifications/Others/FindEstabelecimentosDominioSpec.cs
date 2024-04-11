using Pulsar.Services.Identity.Domain.Aggregates.Others;

namespace Pulsar.Services.Identity.Domain.Specifications.Others;

public class FindEstabelecimentosDominioSpec : IFindSpecification<Estabelecimento>
{
    public FindEstabelecimentosDominioSpec(ObjectId dominioId)
    {
        DominioId = dominioId;
    }

    public ObjectId DominioId { get; }

    public FindSpecification<Estabelecimento> GetSpec()
    {
        return Find.Where<Estabelecimento>(x => x.DominioId == DominioId).Build();
    }
}
