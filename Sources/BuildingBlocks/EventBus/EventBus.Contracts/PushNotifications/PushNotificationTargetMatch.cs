namespace Pulsar.BuildingBlocks.EventBus.Contracts.PushNotifications;

public enum PushNotificationTargetMatch
{
	/// <summary>
	/// Entrega a mensagem para o usuário no domínio ou estabelecimento informado
	/// </summary>
	ExactMatch,
	/// <summary>
	/// Entrega mensagem para o usuário em todos os domínios e estabelecimentos
	/// </summary>
	MatchUsuarioOnly,
	/// <summary>
	/// Entrega mensagem para o usuário em todos os domínios
	/// </summary>
	MatchUsuarioDominio,
	/// <summary>
	/// Entrega mensagem para o usuário em todos os estabelecimentos
	/// </summary>
	MatchUsuarioEstabelecimentos,
	/// <summary>
	/// Entrega mensagem para o usuário em todos os estabelecimentos do domínio informado
	/// </summary>
	MatchUsuarioEstabelecimentosFromDominio
}
