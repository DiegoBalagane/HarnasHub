using HarnasHub.Core.Enums;

namespace HarnasHub.Application.Features.Nades.Shared;

/// <summary>One nade lineup entry.</summary>
public record NadeEntryDto(
	Guid Id,
	MapName MapName,
	string Type,
	string Title,
	string? Description,
	string? YoutubeUrl,
	float? LandingX,
	float? LandingY,
	Guid CreatedByUserId);
