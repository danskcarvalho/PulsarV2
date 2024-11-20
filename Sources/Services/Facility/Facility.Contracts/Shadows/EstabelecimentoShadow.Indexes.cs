using Pulsar.BuildingBlocks.DDD;
using Pulsar.BuildingBlocks.Sync.Contracts;

namespace Pulsar.Services.Facility.Contracts.Shadows;

public partial class EstabelecimentoShadow
{
    public class Indexes : IndexDescriptions<EstabelecimentoShadow>
    {
        public static IX Redes_v1 = Describe.Ascending(e => e.Redes);

        public override string CollectionName => GetCollectionName();
    }
}
