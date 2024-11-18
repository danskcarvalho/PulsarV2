using MongoDB.Bson;

namespace Pulsar.Services.PushNotification.Contracts.Commands.UserContexts;

public class GetUserContextByTokenCmd : IRequest<GetUserContextByTokenResult>
{
	public GetUserContextByTokenCmd(string token)
	{
		Token = token;
	}

	public string Token { get; private init; }
}

public class GetUserContextByTokenResult
{
	public GetUserContextByTokenResult(ObjectId? userContextId)
	{
		UserContextId = userContextId;
	}

	public ObjectId? UserContextId { get; private set; }	
}
