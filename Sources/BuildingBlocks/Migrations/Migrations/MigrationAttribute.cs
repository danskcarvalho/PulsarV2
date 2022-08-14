namespace Pulsar.BuildingBlocks.Migrations;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class MigrationAttribute : Attribute
{
    public long Version { get; }
    public bool RequiresTransaction { get; set; }
    public MigrationAttribute(long version)
    {
        Version = version;
    }
}
