using ErrorOr;
using MediatR;

namespace HarnasHub.Application.Features.Stats.GetMyStatsHistory;

/// <summary>Returns the current user's stat lines across matches, oldest first, for a personal trend view.</summary>
public record GetMyStatsHistoryQuery : IRequest<ErrorOr<List<PlayerStatHistoryEntryDto>>>;

/// <summary>One match's worth of the current user's stats, with the match date for charting.</summary>
public record PlayerStatHistoryEntryDto(
	Guid MatchResultId,
	DateTime PlayedAtUtc,
	string Opponent,
	int Kills,
	int Deaths,
	int Assists,
	double Adr,
	double HeadshotPercentage,
	double Rating);
