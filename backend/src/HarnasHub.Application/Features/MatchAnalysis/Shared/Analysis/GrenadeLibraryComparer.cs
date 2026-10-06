#region Usings

using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Common.Maps;
using HarnasHub.Core.Enums;

#endregion

namespace HarnasHub.Application.Features.MatchAnalysis.Shared.Analysis;

/// <summary>A library nade with a landing pin, the only kind a thrown grenade can be matched against.</summary>
public record LibraryNade(Guid Id, GrenadeType Type, string Title, float X, float Y);

/// <summary>Pure comparison of the grenades our team threw with the team's nade library: a throw "is" a library nade when
/// it is the same type and lands within <see cref="MatchRadius"/> of its pin (radar fractions). Unmatched throws are
/// clustered by the same radius and the repeated ones become candidates for the library.</summary>
public static class GrenadeLibraryComparer
{
	#region Public Fields

	/// <summary>Maximum radar distance between a landing and a library pin to count as the same nade.</summary>
	public const float MatchRadius = 0.03f;

	/// <summary>Minimum throws of one unlisted spot before it is suggested.</summary>
	public const int MinCandidateCount = 2;

	/// <summary>Maximum suggested candidates.</summary>
	public const int MaxCandidates = 8;

	#endregion

	#region Public Methods

	/// <summary>The library type a demo grenade corresponds to, or null for decoys (not trainable).</summary>
	public static GrenadeType? ToLibraryType(DemoGrenadeType type) => type switch
	{
		DemoGrenadeType.Smoke => GrenadeType.Smoke,
		DemoGrenadeType.Flash => GrenadeType.Flash,
		DemoGrenadeType.HighExplosive => GrenadeType.Frag,
		DemoGrenadeType.Molotov or DemoGrenadeType.Incendiary => GrenadeType.Molotov,
		_ => null
	};

	/// <summary>Compares our thrown grenades with the library; null when nothing in the library is pinned on this map.</summary>
	public static GrenadeLibraryComparisonDto? Compare(AnalysisContext context, IReadOnlyList<LibraryNade> library)
	{
		if (library.Count == 0)
		{
			return null;
		}

		var thrown = context.Timeline.Grenades
			.Where(g => g.ThrowerSteamId64 is { } id && context.IsOurs(id) && g.Landing is { RadarX: not null, RadarY: not null })
			.Select(g => (Grenade: g, Type: ToLibraryType(g.Type)))
			.Where(x => x.Type is not null)
			.Select(x => (x.Grenade, Type: x.Type!.Value))
			.ToList();

		var counts = library.ToDictionary(n => n.Id, _ => 0);
		var unmatched = new List<(DemoGrenade Grenade, GrenadeType Type)>();

		foreach (var item in thrown)
		{
			var nade = library.Where(n => n.Type == item.Type && Matches(n.X, n.Y, item.Grenade.Landing!)).MinBy(n => Distance(n.X, n.Y, item.Grenade.Landing!));
			if (nade is null)
			{
				unmatched.Add(item);
			}
			else
			{
				counts[nade.Id]++;
			}
		}

		var usage = library
			.Select(n => new LibraryNadeUsageDto(n.Id, n.Type.ToString(), n.Title, n.X, n.Y, counts[n.Id]))
			.OrderByDescending(u => u.TimesThrown).ThenBy(u => u.Type).ThenBy(u => u.Title)
			.ToList();

		var byType = usage
			.GroupBy(u => u.Type)
			.Select(g => new GrenadeTypeCoverageDto(g.Key, g.Count(), g.Count(u => u.TimesThrown > 0)))
			.OrderBy(t => t.Type)
			.ToList();

		return new GrenadeLibraryComparisonDto(
			library.Count,
			usage.Count(u => u.TimesThrown > 0),
			thrown.Count,
			byType,
			usage,
			Candidates(context, unmatched));
	}

	#endregion

	#region Private Methods

	private static List<UnlistedGrenadeDto> Candidates(AnalysisContext context, List<(DemoGrenade Grenade, GrenadeType Type)> unmatched)
	{
		var clusters = new List<(GrenadeType Type, float X, float Y, DemoPosition Throw, int Count)>();

		foreach (var (grenade, type) in unmatched)
		{
			var landing = grenade.Landing!;
			var index = clusters.FindIndex(c => c.Type == type && Matches(c.X, c.Y, landing));
			if (index >= 0)
			{
				var cluster = clusters[index];
				clusters[index] = cluster with { Count = cluster.Count + 1 };
			}
			else
			{
				clusters.Add((type, landing.RadarX!.Value, landing.RadarY!.Value, grenade.Throw, 1));
			}
		}

		return clusters
			.Where(c => c.Count >= MinCandidateCount)
			.OrderByDescending(c => c.Count)
			.Take(MaxCandidates)
			.Select(c => new UnlistedGrenadeDto(
				c.Type.ToString(),
				c.X,
				c.Y,
				c.Throw.RadarX,
				c.Throw.RadarY,
				c.Count,
				MapZones.Find(context.Timeline.MapName, c.X, c.Y)?.Name))
			.ToList();
	}

	private static bool Matches(float x, float y, DemoPosition landing) => Distance(x, y, landing) <= MatchRadius;

	private static double Distance(float x, float y, DemoPosition landing) =>
		Math.Sqrt(Math.Pow(x - landing.RadarX!.Value, 2) + Math.Pow(y - landing.RadarY!.Value, 2));

	#endregion
}
