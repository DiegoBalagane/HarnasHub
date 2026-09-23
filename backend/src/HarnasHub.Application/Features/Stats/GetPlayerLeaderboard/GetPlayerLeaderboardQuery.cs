using ErrorOr;
using HarnasHub.Core.Enums;
using MediatR;

namespace HarnasHub.Application.Features.Stats.GetPlayerLeaderboard;

/// <summary>Returns every roster player's stats averaged across their matches, for comparing the team against itself.
/// <paramref name="Category"/> narrows it to one kind of game (Scrimmage/League/Tournament); null includes all of them.</summary>
public record GetPlayerLeaderboardQuery(MatchCategory? Category) : IRequest<ErrorOr<List<PlayerLeaderboardEntryDto>>>;

/// <summary>One player's stats averaged across every match counted in the current filter. Everything from
/// <paramref name="AvgKastPercentage"/> onward is null only when none of the player's matches had a demo-imported
/// stat line to compute it from — a manually entered row never carries entry/utility/flash numbers.</summary>
public record PlayerLeaderboardEntryDto(
	Guid UserId,
	string DisplayName,
	string? InGameNickname,
	int MatchesPlayed,
	double AvgKills,
	double AvgDeaths,
	double AvgAssists,
	double AvgAdr,
	double AvgRating,
	double AvgHeadshotPercentage,
	double? AvgKastPercentage,
	double? AvgEntryKills,
	double? AvgEntryDeaths,
	double? AvgUtilityDamage,
	double? AvgFlashAssists);
