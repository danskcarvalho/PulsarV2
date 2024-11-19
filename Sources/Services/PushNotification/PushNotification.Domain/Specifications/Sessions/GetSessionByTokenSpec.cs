using Pulsar.Services.PushNotification.Domain.Aggregates.Sessions;

namespace Pulsar.Services.PushNotification.Domain.Specifications.Sessions;

public class GetSessionByTokenSpec : IFindSpecification<Session>
{
	public GetSessionByTokenSpec(string token)
	{
		Token = token;
	}
	
	public string Token { get; private init; }

	public FindSpecification<Session> GetSpec()
	{
		return Find.Where<Session>(s => s.Token == Token && DateTime.UtcNow < s.ExpiresOn).Limit(1).Build();
	}
}
