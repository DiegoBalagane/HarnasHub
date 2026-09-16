namespace HarnasHub.Application.Features.MapStrategy.Shared;

/// <summary>One player's starting spot on a map side, already joined with the roster data the radar view renders.</summary>
public record MapPositionDto(
	Guid Id,
	string UserId,
	string DisplayName,
	string? InGameNickname,
	string? TeamRole,
	string? Label,
	float X,
	float Y,
	string? Note);
