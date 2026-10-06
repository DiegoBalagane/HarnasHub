#region Usings

using System.Collections.Concurrent;

#endregion

namespace HarnasHub.Application.Features.Tactics.Shared.Matching;

/// <summary>Process-wide cache of extracted <see cref="RoundSignature"/>s per stored timeline (registered as a singleton), so
/// tactic effectiveness — recomputed on every read against the current tactics — downloads each timeline from object storage
/// only once. Keys include the timeline's object key and creation time, so a replaced demo is a new entry; the cache is
/// simply cleared when it outgrows <see cref="MaxEntries"/>.</summary>
public sealed class RoundSignatureCache
{
	#region Public Fields

	/// <summary>Upper bound on cached timelines before the cache is cleared.</summary>
	public const int MaxEntries = 200;

	#endregion

	#region Private Fields

	private readonly ConcurrentDictionary<string, IReadOnlyList<RoundSignature>> _entries = new();

	#endregion

	#region Public Properties

	/// <summary>Number of cached timelines.</summary>
	public int Count => _entries.Count;

	#endregion

	#region Public Methods

	/// <summary>Cache key of one stored timeline as seen with one team as "us".</summary>
	public static string Key(string objectKey, DateTime createdAtUtc, IEnumerable<long> ourTeam) =>
		$"{objectKey}|{createdAtUtc.Ticks}|{string.Join(',', ourTeam.Order())}";

	/// <summary>Returns the cached signatures for <paramref name="key"/> or extracts, caches and returns them.</summary>
	public async Task<IReadOnlyList<RoundSignature>> GetOrAddAsync(string key, Func<Task<IReadOnlyList<RoundSignature>>> factory)
	{
		if (_entries.TryGetValue(key, out var cached))
		{
			return cached;
		}

		var signatures = await factory();
		if (_entries.Count >= MaxEntries)
		{
			_entries.Clear();
		}

		_entries[key] = signatures;
		return signatures;
	}

	#endregion
}
