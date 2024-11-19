using System.Text.Json.Serialization;

namespace Pulsar.Web.Client.Models.PushNotification
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
