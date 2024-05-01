namespace Pulsar.BuildingBlocks.UnitTests.Mocking.MongoDB;

class UpdateVersionAndAlso<TModel>(IUpdateSpecification<TModel> otherSpecification) : IUpdateSpecification<TModel> where TModel : class, IAggregateRoot
{
    public UpdateSpecification<TModel> GetSpec()
    {
        var spec = otherSpecification.GetSpec();
        return Update.Where(spec.Predicate).CopyCommandsFrom(otherSpecification).Inc(x => x.Version, 1).Build();
    }
}