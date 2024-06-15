using System.Diagnostics.CodeAnalysis;

namespace Pulsar.Services.Shared.DTOs;

public class ExceptionDTO
{
    public required List<string> Types { get; set; }
    public string? Key { get; set; }
    public required string Message { get; set; }

    [SetsRequiredMembers]
    public ExceptionDTO(List<string> types, string? key, string message)
    {
        Types = types;
        Key = key;
        Message = message;
    }
    private ExceptionDTO() { }
}
