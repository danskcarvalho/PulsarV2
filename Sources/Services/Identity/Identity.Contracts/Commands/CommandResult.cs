namespace Pulsar.Services.Identity.Contracts.Commands;

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
