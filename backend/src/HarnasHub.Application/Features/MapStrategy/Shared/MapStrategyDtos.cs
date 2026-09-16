namespace HarnasHub.Application.Features.MapStrategy.Shared;

/// <summary>One player's starting spot on a map side, already joined with the roster data the radar view renders; <paramref name="PinColor"/> is the player's self-chosen colour (Main roster only), null when unset.</summary>
public record MapPositionDto(
	Guid Id,
	string UserId,
	string DisplayName,
	string? InGameNickname,
	string? TeamRole,
	string? PinColor,
	string? Label,
	float X,
	float Y,
	string? Note);
