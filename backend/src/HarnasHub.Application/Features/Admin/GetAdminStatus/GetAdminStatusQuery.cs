using ErrorOr;
using MediatR;

namespace HarnasHub.Application.Features.Admin.GetAdminStatus;

/// <summary>Returns which optional integrations are configured on this deployment (Manager only, enforced by the endpoint).</summary>
public record GetAdminStatusQuery : IRequest<ErrorOr<AdminStatusDto>>;

/// <summary>Configuration presence flags only — never the secret values.</summary>
public record AdminStatusDto(
	bool FaceitApiKeyConfigured,
	bool FaceitDownloadsTokenConfigured,
	bool S3Configured,
	bool DiscordWebhookConfigured,
	bool FrontendBaseUrlConfigured,
	DiscordChannelsStatusDto DiscordChannels);

/// <summary>Per-channel Discord webhook flags (true when the channel has its own webhook or the <c>Discord:WebhookUrl</c> fallback).</summary>
public record DiscordChannelsStatusDto(bool Announcements, bool MatchSchedule, bool DemoReview, bool OpponentScouting);
