using System.Linq.Expressions;

namespace Pulsar.BuildingBlocks.DataFactory;

public static class Builder
{
    public static BuilderRoot ForSeed(int seed) => new(seed);
}
