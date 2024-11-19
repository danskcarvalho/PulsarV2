using Microsoft.JSInterop;
using Pulsar.Services.Shared.DTOs;
using System.Collections;
using System.Collections.Specialized;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Web;
using static Microsoft.FluentUI.AspNetCore.Components.Icons.Filled.Size16;
using System.Diagnostics;
using Pulsar.Web.Client.Exceptions;
using Pulsar.Services.Identity.Contracts.DTOs;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using Pulsar.Services.Shared.Commands;

namespace Pulsar.Web.Client.Clients.Base;

public abstract class ClientBase(ClientContext clientContext)
{
	private object _empty = new object();
	protected HttpClient Http => clientContext.HttpClient;

	protected abstract string Section { get; }
	protected abstract string Service { get; }
	protected virtual string Version => "v2";

	protected async Task<TResult> Get<TResult>(string? endpoint, object? parameters = null, string? version = null, CancellationToken cancellationToken = default)
	{
		var url = endpoint != null ?
			$"/api/{Service}/{version ?? Version}/{Section}/{endpoint}" :
			$"/api/{Service}/{version ?? Version}/{Section}";
		url = GetUrl(url, parameters);

		using var r = await clientContext.HttpClient.GetAsync(url, cancellationToken: cancellationToken);
		await HandleExceptions(r, cancellationToken);

		return (await r.Content.ReadFromJsonAsync<TResult>(cancellationToken: cancellationToken, options: SerializationOptions.Default))!;
	}

	private async Task HandleExceptions(HttpResponseMessage r, CancellationToken cancellationToken)
	{
		if (r.StatusCode != System.Net.HttpStatusCode.OK)
		{
			ExceptionDTO? exception = null;
			try
			{
				exception = (await r.Content.ReadFromJsonAsync<ExceptionDTO>(cancellationToken: cancellationToken, options: SerializationOptions.Default))!;
				await clientContext.JsRuntime.InvokeVoidAsync("console.error", exception);
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Error when deserializing exception: {ex.Message}");
				throw new BackendException(r.StatusCode, ex);

			}
			throw new BackendException(r.StatusCode, exception);
		}
	}

	protected async Task<TResult> Delete<TResult>(string? endpoint, object? parameters = null, string? version = null, CancellationToken cancellationToken = default)
	{
		var url = endpoint != null ?
			$"/api/{Service}/{version ?? Version}/{Section}/{endpoint}" :
			$"/api/{Service}/{version ?? Version}/{Section}";
		url = GetUrl(url, parameters);

		using var r = await clientContext.HttpClient.DeleteAsync(url, cancellationToken: cancellationToken);
		await HandleExceptions(r, cancellationToken);

		var result = (await r.Content.ReadFromJsonAsync<TResult>(cancellationToken: cancellationToken, options: SerializationOptions.Default))!;
		HandleConsistencyToken(result);
		return result;
	}

	private void HandleConsistencyToken<TResult>(TResult result)
	{
		if (result is CommandResult cr && cr.ConsistencyToken != null)
		{
			clientContext.ConsistencyTokenManager.OnTokenObserved(cr.ConsistencyToken);
		}
	}

	protected async Task<TResult> Send<TResult>(HttpMethod method, string? endpoint, object? body = null, string? version = null, CancellationToken cancellationToken = default)
	{
		var url = endpoint != null ?
			$"/api/{Service}/{version ?? Version}/{Section}/{endpoint}" :
			$"/api/{Service}/{version ?? Version}/{Section}";

		using var r = method.Method switch
		{
			"POST" => body != null ?
				await clientContext.HttpClient.PostAsJsonAsync(url, body, cancellationToken: cancellationToken, options: SerializationOptions.Default) :
				await clientContext.HttpClient.PostAsync(url, null, cancellationToken: cancellationToken),
			"PUT" => body != null ?
				await clientContext.HttpClient.PutAsJsonAsync(url, body, cancellationToken: cancellationToken, options: SerializationOptions.Default) :
				await clientContext.HttpClient.PutAsync(url, null, cancellationToken: cancellationToken),
			_ => throw new ArgumentException("unknown http method")
		};
		await HandleExceptions(r, cancellationToken);
		var result = (await r.Content.ReadFromJsonAsync<TResult>(cancellationToken: cancellationToken, options: SerializationOptions.Default))!;
		HandleConsistencyToken(result);
		return result;
	}

	protected Task<TResult> Post<TResult>(string? endpoint, object? body = null, string? version = null, CancellationToken cancellationToken = default) =>
		Send<TResult>(HttpMethod.Post, endpoint, body, version, cancellationToken);

	protected Task<TResult> Put<TResult>(string? endpoint, object? body = null, string? version = null, CancellationToken cancellationToken = default) =>
		Send<TResult>(HttpMethod.Put, endpoint, body, version, cancellationToken);

	protected async Task<TResult> SendFiles<TResult>(HttpMethod method,
							 string? endpoint,
							 IReadOnlyDictionary<string, string>? additionalKeys = null,
							 string? version = null,
							 CancellationToken cancellationToken = default,
							 long maxAllowedSizeInBytes = 500 * 1024,
							 params (string Name, IBrowserFile File)[] files)
	{
		try
		{
			using var content = new MultipartFormDataContent();

			foreach (var file in files)
			{
				var fileContent = new StreamContent(file.File.OpenReadStream(maxAllowedSizeInBytes));

				fileContent.Headers.ContentType =
					new MediaTypeHeaderValue(file.File.ContentType);

				content.Add(
					content: fileContent,
					name: file.Name,
					fileName: file.File.Name);
			}

			if (additionalKeys != null)
			{
				var form = new FormUrlEncodedContent(additionalKeys);
				content.Add(form);
			}

			var url = endpoint != null ?
				$"/api/{Service}/{version ?? Version}/{Section}/{endpoint}" :
				$"/api/{Service}/{version ?? Version}/{Section}";

			using var r = method.Method switch
			{
				"POST" => await clientContext.HttpClient.PostAsync(url, content, cancellationToken: cancellationToken),
				"PUT" => await clientContext.HttpClient.PutAsync(url, content, cancellationToken: cancellationToken),
				_ => throw new ArgumentException("unknown http method")
			};
			await HandleExceptions(r, cancellationToken);
			var result = (await r.Content.ReadFromJsonAsync<TResult>(cancellationToken: cancellationToken, options: SerializationOptions.Default))!;
			HandleConsistencyToken(result);
			return result;
		}
		catch (IOException ex)
		{
			throw new BackendException(System.Net.HttpStatusCode.BadRequest, $"Arquivo excede limite de {FormatSize(maxAllowedSizeInBytes)}.", ex);
		}
	}

	private string FormatSize(long bytes)
	{
		const long gb = 1024 * 1024 * 1024;
		const long mb = 1024 * 1024;
		const long kb = 1024;

		return bytes switch
		{
			>= gb => $"{bytes / gb} GB",
			>= mb => $"{bytes / mb} MB",
			>= kb => $"{bytes / kb} KB",
			_ => $"{bytes} bytes"
		};
	}

	protected string GetUrl(string url, object? parameters)
	{

		var queryString = HttpUtility.ParseQueryString(string.Empty);

		if (clientContext.ConsistencyTokenManager.ConsistencyToken != null)
		{
			queryString["consistencyToken"] = clientContext.ConsistencyTokenManager.ConsistencyToken;
		}

		if (parameters != null)
		{
			if (parameters is IDictionary dictionary)
			{
				foreach (var key in dictionary.Keys)
				{
					var val = dictionary[key];

					if (val is IList list)
					{
						foreach (var item in list)
						{
							queryString.Add(
								Convert.ToString(key, CultureInfo.InvariantCulture),
								Convert.ToString(item, CultureInfo.InvariantCulture));
						}
					}
					else
					{
						queryString.Add(
							Convert.ToString(key, CultureInfo.InvariantCulture),
							Convert.ToString(val, CultureInfo.InvariantCulture));
					}
				}
			}
			else
			{
				foreach (var key in parameters.GetType().GetProperties())
				{
					var val = key.GetValue(parameters);

					if (val is IList list)
					{
						foreach (var item in list)
						{
							queryString.Add(
								Convert.ToString(key.Name, CultureInfo.InvariantCulture),
								Convert.ToString(item, CultureInfo.InvariantCulture));
						}
					}
					else
					{
						queryString.Add(
							Convert.ToString(key.Name, CultureInfo.InvariantCulture),
							Convert.ToString(val, CultureInfo.InvariantCulture));
					}
				}
			}
		}

		if (queryString.Count > 0)
		{
			return url.Contains("?") ? url + "&" + queryString.ToString() : url + "?" + queryString.ToString();
		}
		else
		{
			return url;
		}
	}

	protected class SerializationOptions
	{
		public static readonly JsonSerializerOptions Default;

		static SerializationOptions()
		{
			Default = new JsonSerializerOptions();
			Default.PropertyNameCaseInsensitive = true;
			Default.NumberHandling = JsonNumberHandling.AllowReadingFromString;
			Default.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
			Default.Converters.Add(new JsonStringEnumConverter());
		}
	}
}
