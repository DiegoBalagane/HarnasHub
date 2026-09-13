using ErrorOr;
using MediatR;

namespace HarnasHub.Application.Features.Stats.GetTeamTrend;

/// <summary>Returns the team's win-rate trend across logged matches, oldest first.</summary>
public record GetTeamTrendQuery : IRequest<ErrorOr<List<TeamTrendPointDto>>>;

/// <summary>One point on the team trend chart: a match's outcome and the cumulative win rate up to it.</summary>
public record TeamTrendPointDto(DateTime PlayedAtUtc, bool Won, int CumulativeWins, int CumulativeLosses, double WinRatePercentage);
