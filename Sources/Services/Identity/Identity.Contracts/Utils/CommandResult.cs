namespace Pulsar.Services.Identity.Contracts.Utils;

public class CommandResult
{
    public CommandResult(string? consistencyToken)
    {
        ConsistencyToken = consistencyToken;
    }

    public CommandResult()
    {
    }

    public string? ConsistencyToken { get; }
}
