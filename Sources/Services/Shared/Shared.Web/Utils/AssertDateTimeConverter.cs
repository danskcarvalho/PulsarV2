using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Pulsar.Services.Shared.API.Utils;

public class AssertDateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var d = reader.GetDateTime();
        if (d.Kind == DateTimeKind.Unspecified)
            throw new InvalidOperationException("invalid datetime (unspecified kind)");
        else if (d.Kind == DateTimeKind.Local)
            throw new InvalidOperationException("invalid datetime (local kind)");
        return d;
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        if (value.Kind == DateTimeKind.Unspecified)
            throw new InvalidOperationException("invalid datetime (unspecified kind)");
        writer.WriteStringValue(value.ToUniversalTime().ToString("o", CultureInfo.InvariantCulture));
    }
}
