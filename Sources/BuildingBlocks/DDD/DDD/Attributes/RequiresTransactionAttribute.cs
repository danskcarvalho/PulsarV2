namespace Pulsar.BuildingBlocks.DDD.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class RequiresTransactionAttribute : Attribute
{
    public IsolationLevel? IsolationLevel { get; set; }
}
