namespace Pulsar.BuildingBlocks.RedisCaching;

class CachedValue<T> where T : class?
{
    public CachedValue(T? value, bool failed, DateTime expiresOn)
    {
        Value = value;
        Failed = failed;
        ExpiresOn = expiresOn;
    }

    public T? Value { get; }
    public bool Failed { get; }
    public DateTime ExpiresOn { get; }
}
