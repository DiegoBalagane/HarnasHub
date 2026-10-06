#region Usings

using HarnasHub.Core.Enums;

#endregion

namespace HarnasHub.Application.Abstractions;

/// <summary>One contiguous stretch of one player being alive in one official round, sampled once per whole second of round
/// time (counted from freeze end like every other <c>SecondsIntoRound</c>). Sample <c>i</c> is the state at second
/// <c>StartSecond + i</c>; the track ends when the player dies (or the round ends), so "alive" is implicit — a replay draws
/// the player while <see cref="Covers"/> is true. Stored as flat parallel arrays rather than one object per sample to keep
/// the gzip JSON small: <paramref name="World"/> is interleaved x,y,z in whole game units (kept so positions can be
/// re-projected once an uncalibrated map gets a radar fit), <paramref name="Radar"/> interleaved x,y fractions in [0,1]
/// rounded to 4 decimals (null when the map has no calibration), <paramref name="Health"/> HP and
/// <paramref name="Weapons"/> the active weapon's short name ("" when unknown).</summary>
public record DemoPlayerTrack(
	int RoundNumber,
	long SteamId64,
	MapSide Side,
	int StartSecond,
	IReadOnlyList<int> World,
	IReadOnlyList<float>? Radar,
	IReadOnlyList<int> Health,
	IReadOnlyList<string> Weapons)
{
	#region Public Methods

	/// <summary>Whether the track has a sample for <paramref name="second"/> (i.e. the player was alive and seen then).</summary>
	public bool Covers(int second) => second >= StartSecond && second - StartSecond < Health.Count;

	/// <summary>The sample at <paramref name="second"/>, or null when the track doesn't cover it.</summary>
	public DemoTrackSample? At(int second)
	{
		if (!Covers(second))
		{
			return null;
		}

		var i = second - StartSecond;
		return new DemoTrackSample(
			second,
			World[i * 3],
			World[i * 3 + 1],
			World[i * 3 + 2],
			Radar is { } radar ? radar[i * 2] : null,
			Radar is { } r ? r[i * 2 + 1] : null,
			Health[i],
			i < Weapons.Count && Weapons[i].Length > 0 ? Weapons[i] : null);
	}

	/// <summary>The last second this track has a sample for.</summary>
	public int EndSecond() => StartSecond + Health.Count - 1;

	#endregion
}

/// <summary>One decoded sample of a <see cref="DemoPlayerTrack"/> (never serialized — only a convenience view).</summary>
public record DemoTrackSample(
	int Second,
	int WorldX,
	int WorldY,
	int WorldZ,
	float? RadarX,
	float? RadarY,
	int Health,
	string? Weapon);
