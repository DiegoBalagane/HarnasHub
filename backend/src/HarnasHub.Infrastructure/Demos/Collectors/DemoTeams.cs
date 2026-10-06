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
		demo.Players.Where(p => p.CSTeamNum == team).Select(p => (long)p.SteamID).ToList();

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
