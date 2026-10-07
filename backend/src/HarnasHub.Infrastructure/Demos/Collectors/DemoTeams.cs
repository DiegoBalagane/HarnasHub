#region Usings

using DemoFile;
using DemoFile.Game.Cs;
using HarnasHub.Application.Abstractions;
using HarnasHub.Core.Enums;

#endregion

namespace HarnasHub.Infrastructure.Demos.Collectors;

/// <summary>Small team/side helpers shared by the collectors.</summary>
internal static class DemoTeams
{
	#region Public Methods

	/// <summary>Maps a round_end <c>winner</c> value to a side; null for anything that isn't T or CT (aborted round).</summary>
	public static MapSide? WinnerSide(int winner) => winner switch
	{
		(int)CSTeamNumber.Terrorist => MapSide.T,
		(int)CSTeamNumber.CounterTerrorist => MapSide.CT,
		_ => null
	};

	/// <summary>Maps a controller's team to a side; null for spectators/unassigned.</summary>
	public static MapSide? Side(CSTeamNumber team) => team switch
	{
		CSTeamNumber.Terrorist => MapSide.T,
		CSTeamNumber.CounterTerrorist => MapSide.CT,
		_ => null
	};

	/// <summary>SteamID64s currently standing on <paramref name="team"/>. Read straight off each player controller's
	/// current team rather than tracking player_team events: that event only fires on an actual team *change* (e.g. the
	/// halftime swap), so a demo that starts recording after the initial round-1 team joins already happened would see no
	/// side data at all until the first swap. The controller's own state is always current, regardless of when the
	/// recording started.</summary>
	public static List<long> Roster(CsDemoParser demo, CSTeamNumber team) =>
		demo.Players.Where(p => p.CSTeamNum == team && !IsCoach(p)).Select(p => (long)p.SteamID).ToList();

	/// <summary>True for a coach: a controller whose <c>m_iCoachingTeam</c> is set, so it never owns a playing pawn.</summary>
	public static bool IsCoach(CCSPlayerController controller) => (int)controller.CoachingTeam != 0;

	/// <summary>SteamID64s of the non-coach controllers with a live pawn right now (used at freeze end to tell players from spectators).</summary>
	public static HashSet<long> WithLivePawn(CsDemoParser demo) =>
		demo.Players.Where(p => !IsCoach(p) && p.PawnIsAlive).Select(p => (long)p.SteamID).ToHashSet();

	/// <summary>Keeps only roster members seen with a live pawn at freeze end; when nothing was captured (demo started mid-round) the roster is unchanged.</summary>
	public static List<long> FilterPlaying(List<long> roster, IReadOnlySet<long>? presentAtFreezeEnd) =>
		presentAtFreezeEnd is { Count: > 0 } present ? roster.Where(id => id == 0 || present.Contains(id)).ToList() : roster;

	/// <summary>Collapses the game's round-end reason into the competitive subset the app cares about.</summary>
	public static DemoRoundEndReason EndReason(int reason) => (CSRoundEndReason)reason switch
	{
		CSRoundEndReason.TargetBombed => DemoRoundEndReason.BombExploded,
		CSRoundEndReason.BombDefused => DemoRoundEndReason.BombDefused,
		CSRoundEndReason.CTsWin or CSRoundEndReason.TerroristsWin => DemoRoundEndReason.Elimination,
		CSRoundEndReason.TargetSaved => DemoRoundEndReason.TimeExpired,
		CSRoundEndReason.TerroristsSurrender or CSRoundEndReason.CTsSurrender => DemoRoundEndReason.Surrender,
		_ => DemoRoundEndReason.Other
	};

	#endregion
}
