#region Usings

using System.Net.Http.Json;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Common.Notifications;
using HarnasHub.Core.Enums;
using HarnasHub.Core.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

#endregion

namespace HarnasHub.Infrastructure.Notifications;

/// <summary>Posts to a channel's Discord incoming webhook (falling back to <c>Discord:WebhookUrl</c>); silently does nothing when neither is configured.</summary>
public class DiscordWebhookNotifier(HttpClient httpClient, IOptions<DiscordSettings> settings, ILogger<DiscordWebhookNotifier> logger)
	: IDiscordNotifier
{
	#region Public Methods

	/// <inheritdoc />
	public async Task SendAsync(DiscordChannel channel, string message, CancellationToken cancellationToken = default)
	{
		var webhookUrl = settings.Value.GetWebhookUrl(channel);

		if (webhookUrl.Length == 0)
		{
			logger.LogInformation("Discord webhook kanału {Channel} nie jest skonfigurowany — pomijam wiadomość: {Message}", channel, message);
			return;
		}

		try
		{
			var response = await httpClient.PostAsJsonAsync(webhookUrl, new { content = DiscordMessage.Truncate(message) }, cancellationToken);

			if (!response.IsSuccessStatusCode)
			{
				logger.LogWarning("Discord webhook kanału {Channel} zwrócił {StatusCode}", channel, response.StatusCode);
			}
		}
		catch (Exception ex) when (ex is not OperationCanceledException)
		{
			// A failed Discord notification must never break the operation that triggered it.
			logger.LogError(ex, "Nie udało się wysłać wiadomości na Discorda (kanał {Channel})", channel);
		}
	}

	#endregion
}
