using Duende.IdentityServer.Models;

namespace Pulsar.Services.ApiRegistry;

public static class AllApiResources
{
    public readonly static ApiResource[] Resources = new ApiResource[]
    {
        new ApiResource("identity", "Identity API")
        {
            Scopes = AllApiScopes.Resources.Where(s => s.Name.StartsWith("identity.")).Select(s => s.Name).ToList(),
            UserClaims =
            {
               "d", "de", "e", "dp", "ep", "uag", "uad"
            }
        },
        
        new ApiResource("catalog", "Catalog API")
        {
            Scopes = AllApiScopes.Resources.Where(s => s.Name.StartsWith("catalog.")).Select(s => s.Name).ToList(),
            UserClaims =
            {
               "d", "de", "e", "dp", "ep", "uag", "uad"
            }
        },

		new ApiResource("facility", "Facility API")
		{
			Scopes = AllApiScopes.Resources.Where(s => s.Name.StartsWith("facility.")).Select(s => s.Name).ToList(),
			UserClaims =
			{
			   "d", "de", "e", "dp", "ep", "uag", "uad"
			}
		},

		new ApiResource("pushnotification", "Push Notification API")
		{
			Scopes = AllApiScopes.Resources.Where(s => s.Name.StartsWith("pushnotification.")).Select(s => s.Name).ToList(),
			UserClaims =
			{
			   "d", "de", "e", "dp", "ep", "uag", "uad"
			}
		},
	};
}
