using HarnasHub.Core.Enums;

namespace HarnasHub.Application.Features.Nades.Shared;

/// <summary>One nade lineup entry; <c>ThrowX</c>/<c>ThrowY</c> is the radar spot it is thrown from, set only for entries imported from a demo.</summary>
public record NadeEntryDto(
	Guid Id,
	MapName MapName,
	string Type,
	string Title,
	string? Description,
	string? YoutubeUrl,
	float? LandingX,
	float? LandingY,
	Guid CreatedByUserId,
	float? ThrowX,
	float? ThrowY);
