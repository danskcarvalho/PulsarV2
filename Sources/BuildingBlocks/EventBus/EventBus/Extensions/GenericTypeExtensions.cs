namespace Pulsar.BuildingBlocks.EventBus.Extensions;

public static class GenericTypeExtensions
{
    public static string GetGenericTypeName(Type type)
    {
        var typeName = string.Empty;

        if (type.IsGenericType)
        {
            var genericTypes = string.Join(", ", type.GetGenericArguments().Select(t => GetGenericTypeName(t)).ToArray());
            typeName = $"{type.FullName!.Remove(type.FullName.IndexOf('`'))}<{genericTypes}>";
        }
        else
        {
            typeName = type.FullName!;
        }

        return typeName;
    }

    public static string GetGenericTypeName(this object @object)
    {
        return GetGenericTypeName(@object.GetType());
    }
}
