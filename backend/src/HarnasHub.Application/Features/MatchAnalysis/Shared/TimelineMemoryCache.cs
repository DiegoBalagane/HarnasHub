#region Usings

using System.Runtime.CompilerServices;
using HarnasHub.Application.Abstractions;

#endregion

namespace HarnasHub.Application.Features.MatchAnalysis.Shared;

/// <summary>Keeps the most recently read timelines deserialized in memory, so the match page's parallel queries
/// (timeline, insights, analysis, tactic matches, replay) and the map-wide aggregates don't each download and unzip the
/// same object again. Scoped per <see cref="IFileStorage"/> instance — the production storage is a singleton, while each
/// test's fake gets its own cache — and invalidated on every write through <see cref="MatchTimelineStorage"/>.</summary>
internal static class TimelineMemoryCache
{
	#region Private Fields

	// Deserialized timelines with position tracks run to several MB each, so the cache stays small: enough for a map's
	// analytics window (20 matches) plus a few open match pages.
	private const int MaxEntries = 24;

	private static readonly ConditionalWeakTable<IFileStorage, Store> Stores = new();

	#endregion

	#region Public Methods

	/// <summary>Returns the cached timeline for the key or loads, caches and returns it.</summary>
	public static async Task<StoredDemoTimeline> GetOrLoadAsync(
		IFileStorage fileStorage, string objectKey, Func<Task<StoredDemoTimeline>> load)
	{
		var store = Stores.GetValue(fileStorage, _ => new Store());
		if (store.TryGet(objectKey, out var cached))
		{
			return cached;
		}

		var loaded = await load();
		store.Set(objectKey, loaded);
		return loaded;
	}

	/// <summary>Drops the key so the next read goes back to storage (called before the object is overwritten).</summary>
	public static void Forget(IFileStorage fileStorage, string objectKey)
	{
		if (Stores.TryGetValue(fileStorage, out var store))
		{
			store.Remove(objectKey);
		}
	}

	#endregion

	#region Nested Types

	/// <summary>A small least-recently-used map guarded by a lock — reads are rare enough that contention doesn't matter.</summary>
	private sealed class Store
	{
		private readonly Dictionary<string, (StoredDemoTimeline Timeline, long LastUsed)> _entries = [];
		private readonly object _gate = new();
		private long _clock;

		public bool TryGet(string key, out StoredDemoTimeline timeline)
		{
			lock (_gate)
			{
				if (_entries.TryGetValue(key, out var entry))
				{
					_entries[key] = (entry.Timeline, ++_clock);
					timeline = entry.Timeline;
					return true;
				}
			}

			timeline = null!;
			return false;
		}

		public void Set(string key, StoredDemoTimeline timeline)
		{
			lock (_gate)
			{
				if (_entries.Count >= MaxEntries && !_entries.ContainsKey(key))
				{
					var oldest = _entries.MinBy(e => e.Value.LastUsed).Key;
					_entries.Remove(oldest);
				}

				_entries[key] = (timeline, ++_clock);
			}
		}

		public void Remove(string key)
		{
			lock (_gate)
			{
				_entries.Remove(key);
			}
		}
	}

	#endregion
}
