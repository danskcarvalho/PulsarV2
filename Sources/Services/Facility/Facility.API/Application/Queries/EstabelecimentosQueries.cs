

namespace Pulsar.Services.Facility.API.Application.Queries;

public class EstabelecimentosQueries : FacilityQueries, IEstabelecimentoQueries
{
    public EstabelecimentosQueries(FacilityQueriesContext ctx) : base(ctx)
    {
    }
}
