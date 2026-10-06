#region Usings

using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Common.Demos;
using HarnasHub.Application.Common.Maps;
using HarnasHub.Core.Enums;

#endregion

namespace HarnasHub.Application.Features.MatchAnalysis.Shared.Replay;

/// <summary>Who the viewer is in a replayed timeline: <paramref name="Ours"/> are labelled ours, <paramref name="Opponents"/>
/// the opponent; with no explicit opponents, everyone outside a non-empty <paramref name="Ours"/> counts as the opponent.</summary>
public record ReplayPerspective(IReadOnlyCollection<long> Ours, IReadOnlyCollection<long> Opponents, string? OpponentName);

/// <summary>Pure projection of one round of a stored <see cref="DemoTimeline"/> into a <see cref="RoundReplayDto"/>:
/// position tracks become per-second frames, kills/grenades/bomb reference players by index.</summary>
public static class RoundReplayMapper
{
	#region Public Methods

	/// <summary>Builds the replay of round <paramref name="roundNumber"/>, or null when the timeline has no such round.</summary>
	public static RoundReplayDto? Build(DemoTimeline timeline, int roundNumber, ReplayPerspective perspective)
	{
		var round = timeline.Rounds.FirstOrDefault(r => r.Number == roundNumber);
		if (round is null)
		{
			return null;
		}

		var tracks = timeline.Positions.Where(t => t.RoundNumber == roundNumber).ToList();
		var kills = timeline.Kills.Where(k => k.RoundNumber == roundNumber).OrderBy(k => k.SecondsIntoRound).ToList();
		var status = !MapRadarSupport.HasVerifiedRadar(timeline.MapName) ? ReplayPositionsStatus.UncalibratedMap
			: timeline.Positions.Count == 0 ? ReplayPositionsStatus.NotRecorded
			: ReplayPositionsStatus.Available;
		var showPositions = status == ReplayPositionsStatus.Available;

		var roundLength = round.FreezeEndTime is { } freezeEnd
			? Math.Max(0f, round.EndTime - freezeEnd)
			: Math.Max(tracks.Select(t => (float)t.EndSecond()).DefaultIfEmpty(0f).Max(), kills.Select(k => k.SecondsIntoRound).DefaultIfEmpty(0f).Max());
		var duration = Math.Max(1, Math.Max((int)MathF.Ceiling(roundLength), tracks.Select(t => t.EndSecond()).DefaultIfEmpty(0).Max()));

		var players = new PlayerIndex(TimelinePlayerNames.Collect(timeline), perspective);
		foreach (var id in round.TerroristSteamIds)
		{
			players.IndexOf(id, MapSide.T);
		}

		foreach (var id in round.CounterTerroristSteamIds)
		{
			players.IndexOf(id, MapSide.CT);
		}

		var frames = showPositions ? Frames(tracks, duration, players) : [];
		var replayKills = kills.Select(k => new ReplayKillDto(
			k.SecondsIntoRound,
			k.Killer is { } killer ? players.IndexOf(killer.SteamId64, killer.Side) : null,
			players.IndexOf(k.Victim.SteamId64, k.Victim.Side),
			k.Weapon,
			k.Headshot,
			k.IsTeamKill,
			showPositions ? k.Victim.Position?.RadarX : null,
			showPositions ? k.Victim.Position?.RadarY : null)).ToList();

		var grenades = timeline.Grenades
			.Where(g => g.RoundNumber == roundNumber && g.Type != DemoGrenadeType.Decoy)
			.OrderBy(g => g.SecondsIntoRound)
			.Select(g => Grenade(g, roundLength, players, showPositions))
			.ToList();

		var ourSet = perspective.Ours.ToHashSet();
		return new RoundReplayDto(
			timeline.MapName?.ToString(),
			roundNumber,
			timeline.Rounds.Count,
			perspective.OpponentName,
			status,
			duration,
			round.WinnerSide,
			round.EndReason,
			ourSet.Count > 0 ? TimelineTeamResolver.OurSide(round, ourSet) : null,
			players.All,
			frames,
			replayKills,
			grenades,
			Bomb(round, timeline.MapName, roundLength, players, showPositions));
	}

	#endregion

	#region Private Methods

	private static List<ReplayFrameDto> Frames(IReadOnlyList<DemoPlayerTrack> tracks, int duration, PlayerIndex players)
	{
		var frames = new List<ReplayFrameDto>(duration + 1);
		for (var second = 0; second <= duration; second++)
		{
			var states = new List<ReplayPlayerStateDto>();
			foreach (var track in tracks)
			{
				if (track.At(second) is { RadarX: { } x, RadarY: { } y } sample)
				{
					states.Add(new ReplayPlayerStateDto(players.IndexOf(track.SteamId64, track.Side), x, y, sample.Health, sample.Weapon));
				}
			}

			frames.Add(new ReplayFrameDto(second, states));
		}

		return frames;
	}

	private static ReplayGrenadeDto Grenade(DemoGrenade grenade, float roundLength, PlayerIndex players, bool showPositions)
	{
		var (detonate, end, approximate) = GrenadeLifetimes.Resolve(grenade, roundLength);
		var landing = grenade.Landing ?? grenade.Throw;
		return new ReplayGrenadeDto(
			grenade.Id,
			grenade.Type,
			grenade.ThrowerSteamId64 is { } thrower ? players.IndexOf(thrower, grenade.ThrowerSide) : null,
			grenade.ThrowerSide,
			grenade.SecondsIntoRound,
			detonate,
			end,
			approximate,
			showPositions ? grenade.Throw.RadarX : null,
			showPositions ? grenade.Throw.RadarY : null,
			showPositions ? landing.RadarX : null,
			showPositions ? landing.RadarY : null);
	}

	private static ReplayBombDto? Bomb(DemoTimelineRound round, MapName? map, float roundLength, PlayerIndex players, bool showPositions)
	{
		if (round.BombPlant is not { } plant)
		{
			return null;
		}

		var defuse = round.BombDefuse;
		return new ReplayBombDto(
			plant.SecondsIntoRound,
			MatchTimelineMapper.ResolveSite(plant, map),
			plant.PlayerSteamId64 is { } planter ? players.IndexOf(planter, MapSide.T) : null,
			showPositions ? plant.Position?.RadarX : null,
			showPositions ? plant.Position?.RadarY : null,
			defuse?.SecondsIntoRound,
			defuse?.PlayerSteamId64 is { } defuser ? players.IndexOf(defuser, MapSide.CT) : null,
			round.EndReason == DemoRoundEndReason.BombExploded ? roundLength : null);
	}

	#endregion

	#region Nested Types

	/// <summary>Assigns stable indexes to players in order of first appearance.</summary>
	private sealed class PlayerIndex(Dictionary<long, string> names, ReplayPerspective perspective)
	{
		private readonly Dictionary<long, int> _indexes = [];
		private readonly HashSet<long> _ours = perspective.Ours.ToHashSet();
		private readonly HashSet<long> _opponents = perspective.Opponents.ToHashSet();

		public List<ReplayPlayerDto> All { get; } = [];

		public int IndexOf(long steamId64, MapSide? side)
		{
			if (_indexes.TryGetValue(steamId64, out var index))
			{
				return index;
			}

			index = All.Count;
			_indexes[steamId64] = index;
			var name = names.TryGetValue(steamId64, out var known) ? known : steamId64.ToString();
			All.Add(new ReplayPlayerDto(index, steamId64.ToString(), name, side ?? MapSide.T, TeamOf(steamId64)));
			return index;
		}

		private ReplayTeam TeamOf(long steamId64) =>
			_ours.Contains(steamId64) ? ReplayTeam.Ours
			: _opponents.Contains(steamId64) ? ReplayTeam.Opponent
			: _opponents.Count == 0 && _ours.Count > 0 ? ReplayTeam.Opponent
			: ReplayTeam.Unknown;
	}

	#endregion
}
