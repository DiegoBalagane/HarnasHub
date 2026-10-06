#region Usings

using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Common.Maps;
using HarnasHub.Application.Features.MatchAnalysis.Shared;
using HarnasHub.Application.Features.MatchAnalysis.Shared.Analysis;
using HarnasHub.Core.Enums;

#endregion

namespace HarnasHub.Application.Features.Tactics.Shared.Matching;

/// <summary>A radar point (fraction in [0,1]) where one of our players stood during a round's opening window.</summary>
public record SignaturePoint(float X, float Y);

/// <summary>Where one of our grenades landed during a round's opening window; <paramref name="Type"/> is the library type
/// (null for kinds the library doesn't have).</summary>
public record SignatureGrenade(GrenadeType? Type, float X, float Y);

/// <summary>The compact evidence of how we opened one round — everything <see cref="TacticMatcher"/> needs, small enough
/// to cache per stored timeline. <paramref name="HasPositions"/> is false for timelines parsed without position sampling.</summary>
public record RoundSignature(
	int RoundNumber,
	MapSide OurSide,
	bool? WeWon,
	bool HasPositions,
	IReadOnlyList<SignaturePoint> PlayerPoints,
	IReadOnlyList<SignatureGrenade> Grenades);

/// <summary>Pure extraction of <see cref="RoundSignature"/>s for our team from a stored timeline.</summary>
public static class RoundSignatureExtractor
{
	#region Public Fields

	/// <summary>Seconds from freeze end in which our players' positions are compared against a tactic's position points.</summary>
	public const int PositionWindowSeconds = 25;

	/// <summary>Seconds from freeze end in which our grenades are compared against a tactic's grenade points — longer than
	/// the position window because execute utility usually flies between ~20 and ~40 s.</summary>
	public const int GrenadeWindowSeconds = 40;

	#endregion

	#region Public Methods

	/// <summary>One signature per round in which our side is known; empty when the map has no verified radar fit (radar
	/// fractions would be off) or <paramref name="ourTeam"/> is empty.</summary>
	public static List<RoundSignature> Extract(DemoTimeline timeline, IReadOnlyCollection<long> ourTeam)
	{
		if (!MapRadarSupport.HasVerifiedRadar(timeline.MapName) || ourTeam.Count == 0)
		{
			return [];
		}

		var ours = ourTeam.ToHashSet();
		var hasPositions = timeline.Positions.Count > 0;
		var tracksByRound = timeline.Positions.Where(t => ours.Contains(t.SteamId64)).ToLookup(t => t.RoundNumber);
		var grenadesByRound = timeline.Grenades
			.Where(g => g.ThrowerSteamId64 is { } id && ours.Contains(id) && g.Type != DemoGrenadeType.Decoy)
			.ToLookup(g => g.RoundNumber);

		var signatures = new List<RoundSignature>();
		foreach (var round in timeline.Rounds.OrderBy(r => r.Number))
		{
			if (TimelineTeamResolver.OurSide(round, ours) is not { } side)
			{
				continue;
			}

			var points = new List<SignaturePoint>();
			foreach (var track in tracksByRound[round.Number])
			{
				var last = Math.Min(track.EndSecond(), PositionWindowSeconds);
				for (var second = track.StartSecond; second <= last; second++)
				{
					if (track.At(second) is { RadarX: { } x, RadarY: { } y })
					{
						points.Add(new SignaturePoint(x, y));
					}
				}
			}

			var grenades = grenadesByRound[round.Number]
				.Where(g => g.SecondsIntoRound <= GrenadeWindowSeconds && g.Landing is { RadarX: not null, RadarY: not null })
				.Select(g => new SignatureGrenade(GrenadeLibraryComparer.ToLibraryType(g.Type), g.Landing!.RadarX!.Value, g.Landing.RadarY!.Value))
				.ToList();

			bool? weWon = round.WinnerSide is { } winner ? winner == side : null;
			signatures.Add(new RoundSignature(round.Number, side, weWon, hasPositions, points, grenades));
		}

		return signatures;
	}

	#endregion
}
