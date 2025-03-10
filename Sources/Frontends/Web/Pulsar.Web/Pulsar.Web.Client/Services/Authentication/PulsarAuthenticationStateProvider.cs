﻿using System.Net.Http.Json;
using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace Pulsar.Web.Client.Services.Authentication;

public class PulsarAuthenticationStateProvider : AuthenticationStateProvider, IDisposable
{
	private static readonly TimeSpan UserCacheRefreshInterval = TimeSpan.FromSeconds(60);

	private readonly HttpClient _client;
	private readonly ILogger<PulsarAuthenticationStateProvider> _logger;
	Timer? _timer = null;

	private DateTimeOffset _userLastCheck = DateTimeOffset.FromUnixTimeSeconds(0);
	private ClaimsPrincipal _cachedUser = new(new ClaimsIdentity());

	public PulsarAuthenticationStateProvider(
		HttpClient client,
		ILogger<PulsarAuthenticationStateProvider> logger)
	{
		_client = client;
		_logger = logger;
	}

	public override async Task<AuthenticationState> GetAuthenticationStateAsync()
	{
		var user = await GetUser();
		var state = new AuthenticationState(user);

		// checks periodically for a session state change and fires event
		// this causes a round trip to the server
		// adjust the period accordingly if that feature is needed
		if (user.Identity != null && user.Identity.IsAuthenticated && _timer == null)
		{
			//_logger.LogInformation("starting background check..");

			_timer = new Timer(async _ =>
			{
				var currentUser = await GetUser(false);
				if (currentUser.Identity == null || currentUser.Identity.IsAuthenticated == false)
				{
					_logger.LogInformation("user logged out");
					NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(currentUser)));
					await _timer!.DisposeAsync();
					_timer = null;
				}
			}, null, 5000, 30000);
		}

		return state;
	}

	private async ValueTask<ClaimsPrincipal> GetUser(bool useCache = true)
	{
		var now = DateTimeOffset.Now;
		if (useCache && now < _userLastCheck + UserCacheRefreshInterval)
		{
			//_logger.LogDebug("Taking user from cache");
			return _cachedUser;
		}

		//_logger.LogDebug("Fetching user");
		_cachedUser = await FetchUser();
		_userLastCheck = now;

		return _cachedUser;
	}

	record ClaimRecord(string Type, object Value);

	private async Task<ClaimsPrincipal> FetchUser()
	{
		try
		{
			//_logger.LogInformation("Fetching user information.");
			var response = await _client.GetAsync("bff/user?slide=false");

			if (response.StatusCode == HttpStatusCode.OK)
			{
				var claims = await response.Content.ReadFromJsonAsync<List<ClaimRecord>>();

				var identity = new ClaimsIdentity(
					nameof(PulsarAuthenticationStateProvider),
					"name",
					"role");

				if (claims != null)
				{
					foreach (var claim in claims)
					{
						identity.AddClaim(new Claim(claim.Type, claim.Value.ToString()!));
					}
				}

				return new ClaimsPrincipal(identity);
			}
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Fetching user failed.");
		}

		return new ClaimsPrincipal(new ClaimsIdentity());
	}

	public void Dispose()
	{
		if (_timer != null)
		{
			_timer?.Dispose();
			_timer = null;
		}
	}
}
