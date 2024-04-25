namespace Pulsar.BuildingBlocks.Sync.Contracts;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class ShadowAttribute : Attribute
{
    public string Name { get; set; }

    public ShadowAttribute(string name)
    {
        Name = name;
    }
}
