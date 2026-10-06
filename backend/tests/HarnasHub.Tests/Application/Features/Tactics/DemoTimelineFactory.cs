#region Usings

using HarnasHub.Application.Abstractions;
using HarnasHub.Core.Enums;

#endregion

namespace HarnasHub.Tests.Application.Features.Tactics;

/// <summary>Builds small hand-made <see cref="DemoTimeline"/>s for the demo-import tests.</summary>
public static class DemoTimelineFactory
{
	#region Public Fields

	/// <summary>SteamIDs of the team that starts on T.</summary>
	public static readonly long[] TeamA = [1, 2];

	/// <summary>SteamIDs of the team that starts on CT.</summary>
	public static readonly long[] TeamB = [3, 4];

	#endregion

	#region Public Methods

	/// <summary>A round where <paramref name="teamAOnT"/> decides which side team A stands on.</summary>
	public static DemoTimelineRound Round(int number, MapSide? winner, bool teamAOnT = true) => new(
		number, 0f, 15f, 100f, winner, DemoRoundEndReason.Elimination,
		teamAOnT ? TeamA : TeamB,
		teamAOnT ? TeamB : TeamA,
		null, null);

	/// <summary>A radar-positioned grenade.</summary>
	public static DemoGrenade Grenade(
		int id, int round, DemoGrenadeType type = DemoGrenadeType.Smoke, float seconds = 10f, bool hasLanding = true,
		MapSide? side = MapSide.T) => new(
		id, round, type, 1, "nick", side, seconds,
		new DemoPosition(0, 0, 0, 0.1f, 0.2f),
		hasLanding ? new DemoPosition(0, 0, 0, 0.5f, 0.6f) : null);

	/// <summary>A calibrated Mirage timeline with the given rounds and grenades.</summary>
	public static DemoTimeline Timeline(IReadOnlyList<DemoTimelineRound> rounds, IReadOnlyList<DemoGrenade> grenades) =>
		new("de_mirage", MapName.Mirage, true, null, rounds, grenades);

	#endregion
}
