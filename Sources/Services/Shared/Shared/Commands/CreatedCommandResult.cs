namespace Pulsar.Services.Shared.Commands;

public class CreatedCommandResult : CommandResult
{
	/// <summary>
	/// Id da entidade criada.
	/// </summary>
	public string Id { get; set; }

	public CreatedCommandResult(string id)
	{
		Id = id;
	}

	public CreatedCommandResult(string id, string? consistencyToken) : base(consistencyToken)
	{
		Id = id;
	}
}
