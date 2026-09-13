namespace HarnasHub.Application.Features.Tasks.Shared;

/// <summary>A task assigned to a player.</summary>
public record TaskItemDto(
    Guid Id,
    string Title,
    string? Description,
    string Status,
    DateTime? DueAtUtc,
    DateTime CreatedAtUtc);
