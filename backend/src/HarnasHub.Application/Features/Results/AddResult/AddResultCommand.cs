using ErrorOr;
using HarnasHub.Application.Features.Results.Shared;
using HarnasHub.Core.Enums;
using MediatR;

namespace HarnasHub.Application.Features.Results.AddResult;

/// <summary>Logs a scrim/match/tournament result. Coach/Manager only — enforced at the endpoint.
/// <paramref name="TournamentId"/> is set only for <see cref="MatchCategory.Tournament"/>, <paramref name="LeagueId"/> only for <see cref="MatchCategory.League"/>.
/// <paramref name="OurScore"/>/<paramref name="OpponentScore"/>/<paramref name="MapName"/> are plain final values — when the coach analysed
/// a demo first (see <c>AnalyzeDemoCommand</c>), the client already resolved these from it and the coach can still edit them before submitting.
/// <paramref name="DemoPlayers"/>/<paramref name="DemoRoundsPlayed"/> are the same analysis's raw per-player totals, carried back so a stat
/// line can be saved for every one of them without re-uploading the demo file a second time. <paramref name="OurTeamSteamIds"/> is the
/// SteamID64 set of whichever of the analysis's two team splits the coach picked — a stat line is imported for exactly those players,
/// connected to a roster account when one matches and left unconnected (identified only by their demo name) otherwise.</summary>
public record AddResultCommand(
	string Opponent,
	int? OurScore,
	int? OpponentScore,
	string? MapName,
	string? DemoUrl,
	string? Notes,
	DateTime PlayedAtUtc,
	MatchCategory Category,
	Guid? TournamentId,
	Guid? LeagueId,
	int? DemoRoundsPlayed = null,
	IReadOnlyList<AnalyzedDemoPlayerDto>? DemoPlayers = null,
	IReadOnlyList<string>? OurTeamSteamIds = null) : IRequest<ErrorOr<MatchResultDto>>;
