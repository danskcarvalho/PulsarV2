using System.Text.Json;
using System.Text.Json.Serialization;

namespace Pulsar.Services.Identity.UI.Clients
{
    public class SerializationOptions
    {
        public static readonly JsonSerializerOptions Default;

        static SerializationOptions()
        {
            Default = new JsonSerializerOptions();
            Default.PropertyNameCaseInsensitive = true;
            Default.NumberHandling = JsonNumberHandling.AllowReadingFromString;
            Default.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            Default.Converters.Add(new JsonStringEnumConverter());
        }
    }
}
