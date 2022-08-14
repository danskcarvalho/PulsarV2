namespace Pulsar.BuildingBlocks.DDD.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class NoTransactionAttribute : Attribute
{
}
