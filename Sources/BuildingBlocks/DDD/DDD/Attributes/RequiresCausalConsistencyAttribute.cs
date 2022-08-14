namespace Pulsar.BuildingBlocks.DDD.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class RequiresCausalConsistencyAttribute : Attribute
{
    public string? CasualConsistencyTokenProperty { get; set; }
    public bool IfTokenIsPresent { get; set; }
}
