namespace Pulsar.Services.PushNotification.Contracts.Commands.Sessions;

public class CriarSessaoCmd : IRequest<CriarSessaoResult>
{
	public string UsuarioId { get; private set; }
	public string? DominioId { get; set; }
	public string? EstabelecimentoId { get; set; }

	[JsonConstructor]
	public CriarSessaoCmd(string usuarioId)
	{
		UsuarioId = usuarioId;
	}
}

public class CriarSessaoResult : CreatedCommandResult
{
	public string Token { get; private set; }

	public CriarSessaoResult(string id, string token) : base(id)
	{
		Token = token;
	}

	public CriarSessaoResult(string id, string token, string? consistencyToken) : base(id, consistencyToken)
	{
		Token = token;
	}
}
