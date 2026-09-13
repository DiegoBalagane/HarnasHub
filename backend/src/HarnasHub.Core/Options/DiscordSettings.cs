namespace HarnasHub.Core.Options;

/// <summary>Configuration for posting team notifications to Discord.</summary>
public class DiscordSettings
{
    public const string SectionName = "Discord";

    /// <summary>Incoming webhook URL from a Discord channel's Integrations settings. Empty disables notifications.</summary>
    public string WebhookUrl { get; set; } = string.Empty;
}
