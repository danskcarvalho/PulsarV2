using System.ComponentModel.DataAnnotations;

namespace Pulsar.BuildingBlocks.DDD;

public class DomainException : Exception
{
    public static class CommonKeys
    {
        public const string Validation = "Common.Validation";
    }
    public DomainException(string key)
    {
        this.StringKey = key;
    }

    public DomainException(string key, string message)
        : base(message)
    {
        this.StringKey = key;
    }

    public DomainException(string key, string message, Exception innerException)
    : base(message, innerException)
    {
        this.StringKey = key;
    }

    public string StringKey { get; }

    protected static string GetMessageFromKey<TExceptionKey>(TExceptionKey key) where TExceptionKey : notnull
    {
        var enumType = typeof(TExceptionKey);
        var field = enumType.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static).FirstOrDefault(f => f.Name == key.ToString());
        if (field == null)
            return "Erro durante o processamento da sua requisição.";
        var attr = field.GetCustomAttributes(typeof(DisplayAttribute), true).Cast<DisplayAttribute>().FirstOrDefault();
        if (attr == null)
            return "Erro durante o processamento da sua requisição.";
        return attr.Description ?? "Erro durante o processamento da sua requisição.";
    }
}
