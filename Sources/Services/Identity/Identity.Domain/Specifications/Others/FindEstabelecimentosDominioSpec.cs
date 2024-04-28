using Pulsar.Services.Facility.Contracts.Shadows;

namespace Pulsar.Services.Identity.Domain.Specifications.Others;

public class FindEstabelecimentosDominioSpec : IFindSpecification<EstabelecimentoShadow>
{
    public FindEstabelecimentosDominioSpec(ObjectId dominioId)
    {
        DominioId = dominioId;
    }

    public ObjectId DominioId { get; }

    public FindSpecification<EstabelecimentoShadow> GetSpec()
    {
        return Find.Where<EstabelecimentoShadow>(x => x.DominioId == DominioId).Build();
    }
}
