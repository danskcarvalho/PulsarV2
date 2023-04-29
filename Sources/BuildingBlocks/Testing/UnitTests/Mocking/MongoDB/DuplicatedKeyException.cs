namespace Pulsar.BuildingBlocks.UnitTests.Mocking.MongoDB;

public class DuplicatedKeyException : Exception
{
	public DuplicatedKeyException() { }
	public DuplicatedKeyException(string message) : base(message) { }
	public DuplicatedKeyException(string message, Exception inner) : base(message, inner) { }
}
