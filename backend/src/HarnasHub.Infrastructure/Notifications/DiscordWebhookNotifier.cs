using System.Net.Http.Json;
using HarnasHub.Application.Abstractions;
using HarnasHub.Core.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HarnasHub.Infrastructure.Notifications;

/// <summary>Posts to a Discord incoming webhook. Silently does nothing when no webhook URL is configured.</summary>
public class DiscordWebhookNotifier(HttpClient httpClient, IOptions<DiscordSettings> settings, ILogger<DiscordWebhookNotifier> logger)
	: IDiscordNotifier
{
	#region Public Methods

	public async Task SendAsync(string message, CancellationToken cancellationToken = default)
	{
		var webhookUrl = settings.Value.WebhookUrl;

		if (string.IsNullOrWhiteSpace(webhookUrl))
		{
			logger.LogInformation("Discord webhook nie jest skonfigurowany — pomijam wiadomość: {Message}", message);
			return;
		}

		try
		{
			var response = await httpClient.PostAsJsonAsync(webhookUrl, new { content = message }, cancellationToken);

			if (!response.IsSuccessStatusCode)
			{
				logger.LogWarning("Discord webhook zwrócił {StatusCode}", response.StatusCode);
			}
		}
		catch (Exception ex)
		{
			// A failed Discord notification must never break the operation that triggered it.
			logger.LogError(ex, "Nie udało się wysłać wiadomości na Discorda");
		}
	}

	#endregion
}
