#region Usings

using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.MatchAnalysis.Shared;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Application.Features.Tactics;

#endregion

namespace HarnasHub.Tests.Application.Features.MatchAnalysis;

/// <summary>Builds hand-made timelines with economy, kills and bombs for the match-analysis tests. Team A (SteamIDs 1, 2)
/// starts on T, team B (3, 4) on CT — see <see cref="DemoTimelineFactory"/>.</summary>
public static class MatchTimelineFactory
{
	#region Public Methods

	/// <summary>A round with an optional plant at <paramref name="plantSite"/>.</summary>
	public static DemoTimelineRound Round(int number, MapSide winner, bool teamAOnT = true, DemoBombSite? plantSite = null, bool defused = false)
	{
		var round = DemoTimelineFactory.Round(number, winner, teamAOnT);
		if (plantSite is null)
		{
			return round;
		}

		var plant = new DemoBombEvent(40f, 1, "planter", 7, null) { Site = plantSite };
		var defuse = defused ? new DemoBombEvent(70f, 3, "defuser", 7, null) { Site = plantSite } : null;
		return round with { BombPlant = plant, BombDefuse = defuse, EndReason = defused ? DemoRoundEndReason.BombDefused : round.EndReason };
	}

	/// <summary>Both teams' loadouts for a round, with every player of a team carrying the same equipment value.</summary>
	public static DemoRoundEconomy Economy(int number, int teamAPerPlayer, int teamBPerPlayer, bool teamAOnT = true)
	{
		var teamASide = teamAOnT ? MapSide.T : MapSide.CT;
		var teamBSide = teamAOnT ? MapSide.CT : MapSide.T;

		return new DemoRoundEconomy(number,
			DemoTimelineFactory.TeamA.Select(id => Player(id, teamASide, teamAPerPlayer))
				.Concat(DemoTimelineFactory.TeamB.Select(id => Player(id, teamBSide, teamBPerPlayer)))
				.ToList());
	}

	/// <summary>A kill between the given SteamIDs; sides follow the team-A-on-T convention of the round.</summary>
	public static DemoKill Kill(int round, long killer, long victim, bool isOpening = false, bool teamAOnT = true, float seconds = 10f, DemoPosition? victimPosition = null) =>
		new(round, seconds,
			new DemoKillParticipant(killer, $"p{killer}", SideOf(killer, teamAOnT), null),
			new DemoKillParticipant(victim, $"p{victim}", SideOf(victim, teamAOnT), victimPosition),
			null, "ak47", false, false, false, false, false, false, isOpening, false);

	/// <summary>A Mirage timeline with the given rounds, economy and kills.</summary>
	public static DemoTimeline Timeline(
		IReadOnlyList<DemoTimelineRound> rounds,
		IReadOnlyList<DemoRoundEconomy>? economy = null,
		IReadOnlyList<DemoKill>? kills = null) =>
		DemoTimelineFactory.Timeline(rounds, []) with { Economy = economy ?? [], Kills = kills ?? [] };

	/// <summary>Wraps a timeline in a current-version envelope.</summary>
	public static StoredDemoTimeline Stored(DemoTimeline timeline) =>
		new(DemoTimelineSerializer.FormatVersion, DemoTimelineSerializer.CurrentParserVersion, DateTime.UtcNow, timeline);

	/// <summary>A minimal saved match result.</summary>
	public static MatchResult Result(int ourScore = 13, int opponentScore = 10) => new()
	{
		Id = Guid.NewGuid(),
		Opponent = "Team X",
		OurScore = ourScore,
		OpponentScore = opponentScore,
		Category = MatchCategory.Scrimmage,
		PlayedAtUtc = DateTime.UtcNow,
		CreatedByUserId = Guid.NewGuid(),
		CreatedAtUtc = DateTime.UtcNow
	};

	#endregion

	#region Private Methods

	private static MapSide SideOf(long steamId, bool teamAOnT)
	{
		var isTeamA = DemoTimelineFactory.TeamA.Contains(steamId);
		return isTeamA == teamAOnT ? MapSide.T : MapSide.CT;
	}

	private static DemoPlayerEconomy Player(long steamId, MapSide side, int equipment) =>
		new(steamId, $"p{steamId}", side, equipment, 500, equipment, 100, true, false, null);

	#endregion
}
