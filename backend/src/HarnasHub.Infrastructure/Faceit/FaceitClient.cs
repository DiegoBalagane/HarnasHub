using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using HarnasHub.Application.Abstractions;
using HarnasHub.Core.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HarnasHub.Infrastructure.Faceit;

/// <summary>Typed <see cref="HttpClient"/> over the FACEIT Data API v4 with a small retry loop for 429/5xx and transport errors.</summary>
public class FaceitClient(HttpClient httpClient, IOptions<FaceitOptions> options, ILogger<FaceitClient> logger) : IFaceitClient
{
	#region Private Fields

	private const string Game = "cs2";
	private const int MaxAttempts = 3;
	private static readonly TimeSpan MaxRetryDelay = TimeSpan.FromSeconds(30);

	#endregion

	#region Public Properties

	/// <inheritdoc />
	public bool IsConfigured => !string.IsNullOrWhiteSpace(options.Value.ApiKey);

	#endregion

	#region Public Methods

	/// <inheritdoc />
	public async Task<FaceitPlayerInfo?> GetPlayerByNicknameAsync(string nickname, CancellationToken cancellationToken) =>
		await GetAsync($"players?nickname={Uri.EscapeDataString(nickname)}", FaceitJson.ReadPlayer, cancellationToken);

	/// <inheritdoc />
	public async Task<FaceitPlayerInfo?> GetPlayerBySteamIdAsync(string steamId64, CancellationToken cancellationToken) =>
		await GetAsync($"players?game={Game}&game_player_id={Uri.EscapeDataString(steamId64)}", FaceitJson.ReadPlayer, cancellationToken);

	/// <inheritdoc />
	public async Task<FaceitPlayerInfo?> GetPlayerAsync(string playerId, CancellationToken cancellationToken) =>
		await GetAsync($"players/{Uri.EscapeDataString(playerId)}", FaceitJson.ReadPlayer, cancellationToken);

	/// <inheritdoc />
	public async Task<FaceitTeamInfo?> GetTeamAsync(string teamId, CancellationToken cancellationToken) =>
		await GetAsync($"teams/{Uri.EscapeDataString(teamId)}", FaceitJson.ReadTeam, cancellationToken);

	/// <inheritdoc />
	public async Task<FaceitMatchInfo?> GetMatchAsync(string matchId, CancellationToken cancellationToken) =>
		await GetAsync($"matches/{Uri.EscapeDataString(matchId)}", FaceitJson.ReadMatch, cancellationToken);

	/// <inheritdoc />
	public async Task<IReadOnlyList<FaceitHistoryItem>> GetPlayerHistoryAsync(
		string playerId,
		DateTime fromUtc,
		int limit,
		CancellationToken cancellationToken,
		int offset = 0)
	{
		// "from" must be explicit — FACEIT defaults it to one month ago. The endpoint caps a page at 100 items. No "type"
		// filter on purpose: championship (ESEA League), hub and matchmaking rooms all come back.
		var from = new DateTimeOffset(DateTime.SpecifyKind(fromUtc, DateTimeKind.Utc)).ToUnixTimeSeconds();
		var path = $"players/{Uri.EscapeDataString(playerId)}/history?game={Game}&from={from}&offset={Math.Max(0, offset)}&limit={Math.Clamp(limit, 1, 100)}";
		return await GetAsync(path, FaceitJson.ReadHistory, cancellationToken) ?? [];
	}

	/// <inheritdoc />
	public async Task<IReadOnlyList<FaceitLifetimeMapStats>> GetPlayerMapStatsAsync(string playerId, CancellationToken cancellationToken) =>
		await GetAsync($"players/{Uri.EscapeDataString(playerId)}/stats/{Game}", FaceitJson.ReadLifetimeMapStats, cancellationToken) ?? [];

	/// <inheritdoc />
	public async Task<IReadOnlyList<FaceitMapStats>> GetMatchStatsAsync(string matchId, CancellationToken cancellationToken) =>
		await GetAsync($"matches/{Uri.EscapeDataString(matchId)}/stats", FaceitJson.ReadMatchStats, cancellationToken) ?? [];

	#endregion

	#region Private Methods

	/// <summary>GETs <paramref name="relativePath"/> and parses the body; null on 404, retries 429/5xx, throws on anything else.</summary>
	private async Task<T?> GetAsync<T>(string relativePath, Func<JsonElement, T?> read, CancellationToken cancellationToken)
		where T : class
	{
		if (!IsConfigured)
		{
			throw new InvalidOperationException("Brak klucza FACEIT API (Faceit:ApiKey).");
		}

		var uri = new Uri(new Uri(EnsureTrailingSlash(options.Value.BaseUrl)), relativePath);

		for (var attempt = 1; ; attempt++)
		{
			using var request = new HttpRequestMessage(HttpMethod.Get, uri);
			request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", options.Value.ApiKey);
			request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

			HttpResponseMessage response;
			try
			{
				response = await httpClient.SendAsync(request, cancellationToken);
			}
			catch (HttpRequestException ex) when (attempt < MaxAttempts)
			{
				logger.LogWarning(ex, "Błąd połączenia z FACEIT ({Path}), próba {Attempt}", relativePath, attempt);
				await Task.Delay(Backoff(attempt, null), cancellationToken);
				continue;
			}

			using (response)
			{
				if (response.StatusCode == HttpStatusCode.NotFound)
				{
					return null;
				}

				if (response.IsSuccessStatusCode)
				{
					await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
					using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
					return read(document.RootElement);
				}

				var isTransient = response.StatusCode == HttpStatusCode.TooManyRequests || (int)response.StatusCode >= 500;
				if (isTransient && attempt < MaxAttempts)
				{
					logger.LogWarning("FACEIT zwrócił {StatusCode} dla {Path}, ponawiam (próba {Attempt})", (int)response.StatusCode, relativePath, attempt);
					await Task.Delay(Backoff(attempt, response.Headers.RetryAfter), cancellationToken);
					continue;
				}

				logger.LogWarning("FACEIT zwrócił {StatusCode} dla {Path}", (int)response.StatusCode, relativePath);
				throw new HttpRequestException(
					$"FACEIT API zwróciło {(int)response.StatusCode} dla {relativePath}.",
					null,
					response.StatusCode);
			}
		}
	}

	/// <summary>Honours Retry-After when FACEIT sends one, otherwise 1 s, 2 s, 4 s…; never longer than <see cref="MaxRetryDelay"/>.</summary>
	private static TimeSpan Backoff(int attempt, RetryConditionHeaderValue? retryAfter)
	{
		var delay = retryAfter?.Delta
			?? (retryAfter?.Date is { } date ? date - DateTimeOffset.UtcNow : TimeSpan.FromSeconds(Math.Pow(2, attempt - 1)));
		return delay < TimeSpan.Zero ? TimeSpan.Zero : delay > MaxRetryDelay ? MaxRetryDelay : delay;
	}

	/// <summary>Relative paths only resolve under the base path when it ends with "/".</summary>
	private static string EnsureTrailingSlash(string baseUrl) =>
		baseUrl.EndsWith('/') ? baseUrl : baseUrl + "/";

	#endregion
}
