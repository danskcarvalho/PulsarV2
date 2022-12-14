using System.Text.Json.Serialization;

namespace Pulsar.Services.Identity.Contracts.DTOs;

public class ExceptionDTO
{
    public List<string> Types { get; set; }
    public string? Key { get; set; }
    public string Message { get; set; }

    [JsonConstructor]
    public ExceptionDTO(List<string> types, string? key, string message)
    {
        Types = types;
        Key = key;
        Message = message;
    }
}
