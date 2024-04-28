using Pulsar.Services.Facility.Contracts.Shadows;

namespace Pulsar.Services.Identity.Domain.Specifications.Others;

public class FindRedesEstabelecimentosDominioSpec : IFindSpecification<RedeEstabelecimentosShadow>
{
    public FindRedesEstabelecimentosDominioSpec(ObjectId dominioId)
    {
        DominioId = dominioId;
    }

    public ObjectId DominioId { get; }

    public FindSpecification<RedeEstabelecimentosShadow> GetSpec()
    {
        return Find.Where<RedeEstabelecimentosShadow>(x => x.DominioId == DominioId).Build();
    }
}
