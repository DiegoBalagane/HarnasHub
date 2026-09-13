namespace HarnasHub.Application.Features.Results.Shared;

/// <summary>A logged match/scrim/tournament result.</summary>
public record MatchResultDto(
    Guid Id,
    string Opponent,
    int OurScore,
    int OpponentScore,
    string? MapName,
    string? DemoUrl,
    string? Notes,
    DateTime PlayedAtUtc);
