#region Usings

using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;

#endregion

namespace HarnasHub.Application.Features.Tactics.Shared;

/// <summary>Texts and duplicate detection for grenades imported from a demo into a tactic / the nade library.</summary>
public static class DemoNadeFormatter
{
	#region Constants

	/// <summary>Two landings closer than this (in radar fractions, ~2% of the radar) count as the same lineup.</summary>
	public const float DuplicateLandingDistance = 0.02f;

	private const int MaxTitleLength = 100;

	#endregion

	#region Public Methods

	/// <summary>Short label of a grenade type as shown in descriptions, e.g. "HE" for <see cref="GrenadeType.Frag"/>.</summary>
	public static string Label(GrenadeType type) => type switch
	{
		GrenadeType.Smoke => "Smoke",
		GrenadeType.Flash => "Flash",
		GrenadeType.Molotov => "Molotov",
		GrenadeType.Frag => "HE",
		_ => type.ToString()
	};

	/// <summary>Seconds as "m:ss", e.g. 14.7 → "0:14".</summary>
	public static string FormatTime(float seconds)
	{
		var whole = (int)Math.Max(0f, MathF.Floor(seconds));
		return $"{whole / 60}:{whole % 60:00}";
	}

	/// <summary>Tactic point description, e.g. "Smoke · nick · 0:14".</summary>
	public static string PointDescription(GrenadeType type, string throwerName, float seconds) =>
		$"{Label(type)} · {throwerName} · {FormatTime(seconds)}";

	/// <summary>Nade library title, e.g. "Smoke – nick (z demki)", with the nick shortened to fit the title limit.</summary>
	public static string NadeTitle(GrenadeType type, string throwerName)
	{
		var prefix = $"{Label(type)} – ";
		const string suffix = " (z demki)";
		var room = MaxTitleLength - prefix.Length - suffix.Length;
		var nick = throwerName.Length > room ? throwerName[..room] : throwerName;
		return prefix + nick + suffix;
	}

	/// <summary>Finds an existing entry of the same map and type whose landing pin lies within
	/// <see cref="DuplicateLandingDistance"/> of the given point (the nearest one if several); null if none.</summary>
	public static NadeEntry? FindDuplicate(IEnumerable<NadeEntry> entries, MapName mapName, GrenadeType type, float landX, float landY)
	{
		NadeEntry? best = null;
		var bestDistance = DuplicateLandingDistance * DuplicateLandingDistance;

		foreach (var entry in entries)
		{
			if (entry.MapName != mapName || entry.Type != type || entry.LandingX is not { } x || entry.LandingY is not { } y)
			{
				continue;
			}

			var dx = x - landX;
			var dy = y - landY;
			var distance = dx * dx + dy * dy;

			if (distance <= bestDistance)
			{
				bestDistance = distance;
				best = entry;
			}
		}

		return best;
	}

	#endregion
}
