namespace HarnasHub.Application.Features.Stats.Shared;

/// <summary>One player's stat line for one match, with their display name for the UI.</summary>
public record PlayerMatchStatDto(
    Guid Id,
    Guid UserId,
    string DisplayName,
    int Kills,
    int Deaths,
    int Assists,
    double Adr,
    double HeadshotPercentage,
    double Rating);
