namespace HarnasHub.Application.Features.MapStrategy.Shared;

/// <summary>One player's starting spot on a map side, already joined with the roster data the radar view renders; <paramref name="PinColor"/> is the player's self-chosen colour (Main roster only), null when unset; <paramref name="PinMark"/> is a self-chosen single character shown on the pin instead of initials, null when unset.</summary>
public record MapPositionDto(
	Guid Id,
	string UserId,
	string DisplayName,
	string? InGameNickname,
	string? TeamRole,
	string? PinColor,
	string? PinMark,
	string? Label,
	float X,
	float Y,
	string? Note);

/// <summary>A free-floating text label on the radar — not tied to a player, e.g. a callout note.</summary>
public record MapTextAnnotationDto(
	Guid Id,
	string Text,
	string Color,
	int FontSizePx,
	float X,
	float Y);
