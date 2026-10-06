#region Usings

using HarnasHub.Application.Features.MatchAnalysis.Shared;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Common;
using Xunit;
using static HarnasHub.Tests.Application.Features.MatchAnalysis.MatchTimelineFactory;

#endregion

namespace HarnasHub.Tests.Application.Features.MatchAnalysis.Shared;

public class MatchTimelineStorageCacheTests
{
	#region Public Methods

	[Fact]
	public async Task Should_serve_a_repeated_read_from_memory()
	{
		var storage = new TestFileStorage();
		var key = MatchTimelineStorage.MatchKey(Guid.NewGuid());
		await MatchTimelineStorage.SaveAsync(storage, key, Timeline([Round(1, MapSide.T)]), 4, CancellationToken.None);

		var first = await MatchTimelineStorage.LoadAsync(storage, key, CancellationToken.None);
		storage.Objects.Remove(key);
		var second = await MatchTimelineStorage.LoadAsync(storage, key, CancellationToken.None);

		Assert.Same(first, second);
	}

	[Fact]
	public async Task Should_read_the_new_timeline_after_it_is_overwritten()
	{
		var storage = new TestFileStorage();
		var key = MatchTimelineStorage.MatchKey(Guid.NewGuid());
		await MatchTimelineStorage.SaveAsync(storage, key, Timeline([Round(1, MapSide.T)]), 4, CancellationToken.None);
		await MatchTimelineStorage.LoadAsync(storage, key, CancellationToken.None);

		await MatchTimelineStorage.SaveAsync(storage, key, Timeline([Round(1, MapSide.T), Round(2, MapSide.CT)]), 4, CancellationToken.None);
		var reloaded = await MatchTimelineStorage.LoadAsync(storage, key, CancellationToken.None);

		Assert.Equal(2, reloaded.Timeline.Rounds.Count);
	}

	[Fact]
	public async Task Should_not_share_cached_timelines_between_storage_instances()
	{
		var key = MatchTimelineStorage.MatchKey(Guid.NewGuid());
		var firstStorage = new TestFileStorage();
		var secondStorage = new TestFileStorage();
		await MatchTimelineStorage.SaveAsync(firstStorage, key, Timeline([Round(1, MapSide.T)]), 4, CancellationToken.None);
		await MatchTimelineStorage.SaveAsync(secondStorage, key, Timeline([Round(1, MapSide.T), Round(2, MapSide.T)]), 4, CancellationToken.None);

		var fromFirst = await MatchTimelineStorage.LoadAsync(firstStorage, key, CancellationToken.None);
		var fromSecond = await MatchTimelineStorage.LoadAsync(secondStorage, key, CancellationToken.None);

		Assert.Single(fromFirst.Timeline.Rounds);
		Assert.Equal(2, fromSecond.Timeline.Rounds.Count);
	}

	#endregion
}
