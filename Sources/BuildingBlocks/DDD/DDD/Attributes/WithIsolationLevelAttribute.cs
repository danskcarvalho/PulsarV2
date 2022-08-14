namespace Pulsar.BuildingBlocks.DDD.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class WithIsolationLevelAttribute : Attribute
{
    public IsolationLevel IsolationLevel { get; }

    public WithIsolationLevelAttribute(IsolationLevel isolationLevel)
    {
        IsolationLevel = isolationLevel;
    }
}
