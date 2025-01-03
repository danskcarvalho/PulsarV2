﻿namespace Pulsar.Services.Shared.Commands;

public class CommandResult
{
	public CommandResult(string? consistencyToken)
	{
		ConsistencyToken = consistencyToken;
	}

	public CommandResult()
	{
	}

	/// <summary>
	/// Token de consistência.
	/// </summary>
	public string? ConsistencyToken { get; set; }
}
