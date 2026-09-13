namespace HarnasHub.Application.Features.Nades.Shared;

/// <summary>One nade lineup entry.</summary>
public record NadeEntryDto(
    Guid Id,
    string MapName,
    string Type,
    string Title,
    string? Description,
    string? YoutubeUrl,
    Guid CreatedByUserId);
