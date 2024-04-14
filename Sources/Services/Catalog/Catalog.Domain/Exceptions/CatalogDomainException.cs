using Pulsar.BuildingBlocks.DDD;

namespace Pulsar.Services.Identity.Domain.Exceptions;

public class CatalogDomainException : DomainException
{
    public ExceptionKey Key { get; }

    public CatalogDomainException(ExceptionKey key) : this(key, GetMessageFromKey(key))
    {
    }

    public CatalogDomainException(ExceptionKey key, string message)
        : base(message)
    {
        Key = key;
    }

    public CatalogDomainException(ExceptionKey key, string message, Exception innerException)
        : base(message, innerException)
    {
        Key = key;
    }

    private static string GetMessageFromKey(ExceptionKey key)
    {
        var enumType = typeof(ExceptionKey);
        var field = enumType.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static).FirstOrDefault(f => f.Name == key.ToString());
        if (field == null)
            return "Erro durante o processamento da sua requisição.";
        var attr = field.GetCustomAttributes(typeof(DisplayAttribute), true).Cast<DisplayAttribute>().FirstOrDefault();
        if (attr == null)
            return "Erro durante o processamento da sua requisição.";
        return attr.Description ?? "Erro durante o processamento da sua requisição.";
    }
}
