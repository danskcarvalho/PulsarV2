using Pulsar.Services.PushNotification.Contracts.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulsar.Services.PushNotification.Contracts.Queries;

public interface INotificacaoPushQueries
{
	Task<List<NotificacaoPushDTO>> GetNotificacoes(
		string usuarioId,
		string? dominioId,
		string? estabelecimentoId,
		bool excluirLidas,
		string? consistencyToken);
}
