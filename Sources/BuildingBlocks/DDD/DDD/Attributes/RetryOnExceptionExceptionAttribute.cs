namespace Pulsar.BuildingBlocks.DDD.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class RetryOnExceptionExceptionAttribute : Attribute
{
    public RetryOnExceptionExceptionAttribute()
    {
        ExceptionTypes = new List<Type>().AsReadOnly();
    }
    public RetryOnExceptionExceptionAttribute(params Type[] exceptionTypes)
    {
        ExceptionTypes = new List<Type>(exceptionTypes).AsReadOnly();
    }
    public bool DuplicatedKey { get; set; }
    public int? Retries { get; set; }
    public IReadOnlyList<Type> ExceptionTypes { get; }
    public bool VersionConcurrency { get; set; }
}
