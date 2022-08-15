namespace Pulsar.BuildingBlocks.DDD.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class RetryOnExceptionAttribute : Attribute
{
    public RetryOnExceptionAttribute()
    {
        ExceptionTypes = new List<Type>().AsReadOnly();
    }
    public RetryOnExceptionAttribute(params Type[] exceptionTypes)
    {
        ExceptionTypes = new List<Type>(exceptionTypes).AsReadOnly();
    }
    public bool DuplicatedKey { get; set; }
    public int Retries { get; set; }
    public IReadOnlyList<Type> ExceptionTypes { get; }
    public bool VersionConcurrency { get; set; }
}
