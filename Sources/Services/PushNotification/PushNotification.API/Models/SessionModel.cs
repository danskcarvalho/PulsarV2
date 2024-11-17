namespace Pulsar.Services.PushNotification.API.Models
{
	public class SessionModel
	{
		public string Token { get; set; }
		public string Url { get; set; }

		[JsonConstructor]
		public SessionModel(string token, string url)
		{
			Token = token;
			Url = url;
		}
	}
}
