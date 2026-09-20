namespace HarnasHub.Application.Features.Tournaments.Shared;

/// <summary>A named tournament that groups several match results together.</summary>
public record TournamentDto(Guid Id, string Name);
