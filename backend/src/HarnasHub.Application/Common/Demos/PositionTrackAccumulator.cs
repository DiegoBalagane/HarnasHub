#region Usings

using HarnasHub.Application.Abstractions;
using HarnasHub.Core.Enums;

#endregion

namespace HarnasHub.Application.Common.Demos;

/// <summary>Pure bookkeeping behind the position sampler: collects per-second samples of alive players into contiguous
/// <see cref="DemoPlayerTrack"/>s. A player missing from a second (dead, disconnected) closes their track; seeing them
/// again later in the same round opens a new one, so every track's arrays stay gap-free. Kept out of the parser so the
/// rules can be unit-tested without a demo file.</summary>
public sealed class PositionTrackAccumulator
{
	#region Private Types

	private sealed class OpenTrack(int roundNumber, long steamId64, MapSide side, int startSecond)
	{
		public int RoundNumber { get; } = roundNumber;
		public long SteamId64 { get; } = steamId64;
		public MapSide Side { get; } = side;
		public int StartSecond { get; } = startSecond;
		public int LastSecond { get; set; } = startSecond - 1;
		public List<(float X, float Y, float Z)> World { get; } = [];
		public List<int> Health { get; } = [];
		public List<string> Weapons { get; } = [];
	}

	#endregion

	#region Private Fields

	private readonly List<OpenTrack> _closed = [];
	private readonly Dictionary<long, OpenTrack> _open = [];

	#endregion

	#region Public Methods

	/// <summary>Records that <paramref name="steamId64"/> was alive at <paramref name="second"/> of round
	/// <paramref name="roundNumber"/>; call once per player per sampled second, then <see cref="EndSecond"/>.</summary>
	public void Add(int roundNumber, int second, long steamId64, MapSide side, float worldX, float worldY, float worldZ, int health, string? weapon)
	{
		if (!_open.TryGetValue(steamId64, out var track) || track.RoundNumber != roundNumber || track.Side != side || track.LastSecond != second - 1)
		{
			if (track is not null)
			{
				Close(steamId64);
			}

			track = new OpenTrack(roundNumber, steamId64, side, second);
			_open[steamId64] = track;
		}

		track.World.Add((worldX, worldY, worldZ));
		track.Health.Add(Math.Clamp(health, 0, 100));
		track.Weapons.Add(weapon ?? string.Empty);
		track.LastSecond = second;
	}

	/// <summary>Closes the track of every player who got no sample at <paramref name="second"/>.</summary>
	public void EndSecond(int second)
	{
		foreach (var steamId in _open.Where(kv => kv.Value.LastSecond != second).Select(kv => kv.Key).ToList())
		{
			Close(steamId);
		}
	}

	/// <summary>Closes every open track (round over).</summary>
	public void EndRound()
	{
		foreach (var steamId in _open.Keys.ToList())
		{
			Close(steamId);
		}
	}

	/// <summary>Drops everything gathered so far (a match restart).</summary>
	public void Reset()
	{
		_open.Clear();
		_closed.Clear();
	}

	/// <summary>Finished tracks; <paramref name="toRadar"/> projects a world x/y to a radar fraction (null when uncalibrated).</summary>
	public List<DemoPlayerTrack> Build(Func<float, float, (float X, float Y)?> toRadar)
	{
		EndRound();
		return _closed.Select(track => ToTrack(track, toRadar)).ToList();
	}

	#endregion

	#region Private Methods

	private void Close(long steamId64)
	{
		if (_open.Remove(steamId64, out var track) && track.Health.Count > 0)
		{
			_closed.Add(track);
		}
	}

	private static DemoPlayerTrack ToTrack(OpenTrack track, Func<float, float, (float X, float Y)?> toRadar)
	{
		var world = new List<int>(track.World.Count * 3);
		var radar = new List<float>(track.World.Count * 2);
		var calibrated = true;

		foreach (var (x, y, z) in track.World)
		{
			world.Add((int)MathF.Round(x));
			world.Add((int)MathF.Round(y));
			world.Add((int)MathF.Round(z));

			if (calibrated && toRadar(x, y) is { } fraction)
			{
				radar.Add(MathF.Round(fraction.X, 4));
				radar.Add(MathF.Round(fraction.Y, 4));
			}
			else
			{
				calibrated = false;
			}
		}

		return new DemoPlayerTrack(
			track.RoundNumber,
			track.SteamId64,
			track.Side,
			track.StartSecond,
			world,
			calibrated ? radar : null,
			track.Health,
			track.Weapons);
	}

	#endregion
}
