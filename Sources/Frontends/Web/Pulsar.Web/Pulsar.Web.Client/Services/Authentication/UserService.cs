using Blazored.SessionStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Pulsar.Services.Identity.Contracts.Commands.Usuarios;
using Pulsar.Services.Identity.Contracts.Enumerations;
using Pulsar.Web.Client.Clients.Identity.Dominios;
using Pulsar.Web.Client.Clients.Identity.Estabelecimentos;
using Pulsar.Web.Client.Clients.Identity.Usuarios;
using Pulsar.Web.Client.Models.Shared;
using System.Collections.Immutable;
using System.Security.Claims;

namespace Pulsar.Web.Client.Services.Authentication;

public class UserService(AuthenticationStateProvider authenticationStateProvider, IUsuarioClient usuarioClient, ISessionStorageService sessionStorage)
{
	private const string PRIMEIRO_NOME = "UserService.PrimeiroNome";
	private const string ULTIMO_NOME = "UserService.UltimoNome";
	private const string AVATAR_URL = "UserService.AvatarUrl";

	private string? _primeiroNome = null;
	private string? _ultimoNome = null;
	private string? _avatarUrl = null;
	private bool _loadOnce = false;
	public async Task<UserClaims?> CurrentUser()
	{
		await LoadDataFromSession(sessionStorage);
		var authenticated = await authenticationStateProvider.GetAuthenticationStateAsync();

		if (authenticated == null)
		{
			return null;
		}

		var userClaims = GetUserClaims(authenticated);
		if (!_loadOnce)
		{
			LoadBasicData();
		}
		return userClaims;
	}

	private async void LoadBasicData()
	{
		try
		{
			var dados = await usuarioClient.Logado();
			var changed = _primeiroNome != dados.PrimeiroNome || _ultimoNome != dados.UltimoNome || _avatarUrl != dados.AvatarUrl;
			_primeiroNome = dados.PrimeiroNome;
			_ultimoNome = dados.UltimoNome;
			_avatarUrl = dados.AvatarUrl;
			_loadOnce = true;
			await sessionStorage.SetItemAsync(PRIMEIRO_NOME, _primeiroNome);
			await sessionStorage.SetItemAsync(ULTIMO_NOME, _ultimoNome);
			await sessionStorage.SetItemAsync(AVATAR_URL, _avatarUrl);
			if (changed)
			{
				UserChanged?.Invoke(this, EventArgs.Empty);
			}
		}
		catch
		{

		}
	}

	private async Task LoadDataFromSession(ISessionStorageService sessionStorage)
	{
		if (_primeiroNome == null)
		{
			_primeiroNome = await sessionStorage.GetItemAsync<string>(PRIMEIRO_NOME);
		}
		if (_ultimoNome == null)
		{
			_ultimoNome = await sessionStorage.GetItemAsync<string>(ULTIMO_NOME);
		}
		if (_avatarUrl == null)
		{
			_avatarUrl = await sessionStorage.GetItemAsync<string>(AVATAR_URL);
		}
	}

	public event EventHandler? UserChanged;

	private UserClaims? GetUserClaims(AuthenticationState authenticated)
	{
		if (authenticated == null || authenticated.User == null)
		{
			return null;
		}

		var sub = authenticated.User.Claims.FirstOrDefault(x => x.Type == "sub");

		if (sub == null)
		{
			return null;
		}

		var (dominioId, estabelecimentoId, userId) = ParseSub(sub.Value);

		return new UserClaims(
			userId,
			estabelecimentoId,
			dominioId,
			_primeiroNome ?? NullIfEmpty(authenticated.User.Claims.FirstOrDefault(x => x.Type == "first_name")?.Value) ?? "Sem Primeiro Nome",
			_ultimoNome ?? NullIfEmpty(authenticated.User.Claims.FirstOrDefault(x => x.Type == "last_name")?.Value),
			NullIfEmpty(authenticated.User.Claims.FirstOrDefault(x => x.Type == "email")?.Value),
			NullIfEmpty(authenticated.User.Claims.FirstOrDefault(x => x.Type == "username")?.Value),
			_avatarUrl ?? NullIfEmpty(authenticated.User.Claims.FirstOrDefault(x => x.Type == "avatar_url")?.Value),
			NullIfEmpty(authenticated.User.Claims.FirstOrDefault(x => x.Type == "en")?.Value),
			NullIfEmpty(authenticated.User.Claims.FirstOrDefault(x => x.Type == "dn")?.Value),
			authenticated.User.Claims.FirstOrDefault(x => x.Type == "uag")?.Value == "true",
			authenticated.User.Claims.FirstOrDefault(x => x.Type == "uad")?.Value == "true",
			authenticated.User.Claims.FirstOrDefault(x => x.Type == "dp")?.Value.Split(',', StringSplitOptions.RemoveEmptyEntries)
				.Select(s => (PermissoesDominio)int.Parse(s)).ToImmutableHashSet() ?? ImmutableHashSet<PermissoesDominio>.Empty,
			authenticated.User.Claims.FirstOrDefault(x => x.Type == "ep")?.Value.Split(',', StringSplitOptions.RemoveEmptyEntries)
				.Select(s => (PermissoesEstabelecimento)int.Parse(s)).ToImmutableHashSet() ?? ImmutableHashSet<PermissoesEstabelecimento>.Empty);
	}

	private string? NullIfEmpty(string? value)
	{
		if (string.IsNullOrWhiteSpace(value))
		{
			return null;
		}
		else
		{
			return value;
		}
	}

	private (string? dominioId, string? estabelecimentoId, string userId) ParseSub(string sub)
	{
		var parts = sub.Split('/');

		return (GetPartFromSub(parts, 0), GetPartFromSub(parts, 1), GetPartFromSub(parts, 2)!);
	}

	private string? GetPartFromSub(string[] parts, int idx)
	{
		if (idx >= parts.Length)
		{
			return null;
		}

		return parts[idx] == "_" ? null : parts[idx].Trim();
	}

	public async Task EditarMeusDados(EditarMeusDadosCmd cmd)
	{
		await usuarioClient.EditarMeusDados(cmd);
		_primeiroNome = cmd.PrimeiroNome;
		_ultimoNome = cmd.Sobrenome;
		await sessionStorage.SetItemAsync(PRIMEIRO_NOME, _primeiroNome);
		await sessionStorage.SetItemAsync(ULTIMO_NOME, _ultimoNome);
		UserChanged?.Invoke(this, EventArgs.Empty);
	}
	public async Task MudarMeuAvatar(string contentType, FileInfo file)
	{
		await usuarioClient.MudarMeuAvatar(new BrowserFile(contentType, file));
		var dados = await usuarioClient.Logado();
		await sessionStorage.SetItemAsync(AVATAR_URL, dados.AvatarUrl);
		_avatarUrl = dados.AvatarUrl;
		UserChanged?.Invoke(this, EventArgs.Empty);
	}
}

public record UserClaims
(
	string UserId,
	string? EstabelecimentoId,
	string? DominioId,
	string PrimeiroNome,
	string? UltimoNome,
	string? Email,
	string? NomeUsuario,
	string? AvatarUrl,
	string? EstabelecimentoNome,
	string? DominioNome,
	bool IsSuperUsuario,
	bool IsAdministradorDominio,
	ImmutableHashSet<PermissoesDominio> PermissoesDominio,
	ImmutableHashSet<PermissoesEstabelecimento> PermissoesEstabelecimento
);
