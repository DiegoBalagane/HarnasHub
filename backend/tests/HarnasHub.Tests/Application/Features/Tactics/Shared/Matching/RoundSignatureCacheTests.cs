#region Usings

using HarnasHub.Application.Features.Tactics.Shared.Matching;
using Xunit;

#endregion

namespace HarnasHub.Tests.Application.Features.Tactics.Shared.Matching;

public class RoundSignatureCacheTests
{
	#region Public Methods

	[Fact]
	public async Task Should_run_the_factory_only_once_per_key()
	{
		var cache = new RoundSignatureCache();
		var calls = 0;
		Task<IReadOnlyList<RoundSignature>> Factory()
		{
			calls++;
			return Task.FromResult<IReadOnlyList<RoundSignature>>([]);
		}

		await cache.GetOrAddAsync("a", Factory);
		await cache.GetOrAddAsync("a", Factory);
		await cache.GetOrAddAsync("b", Factory);

		Assert.Equal(2, calls);
	}

	[Fact]
	public void Should_key_by_object_creation_time_and_team_regardless_of_order()
	{
		var created = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		Assert.Equal(RoundSignatureCache.Key("k", created, [2, 1]), RoundSignatureCache.Key("k", created, [1, 2]));
		Assert.NotEqual(RoundSignatureCache.Key("k", created, [1]), RoundSignatureCache.Key("k", created.AddMinutes(1), [1]));
	}

	[Fact]
	public async Task Should_clear_itself_once_it_grows_past_the_limit()
	{
		var cache = new RoundSignatureCache();
		for (var i = 0; i <= RoundSignatureCache.MaxEntries; i++)
		{
			await cache.GetOrAddAsync($"k{i}", () => Task.FromResult<IReadOnlyList<RoundSignature>>([]));
		}

		Assert.Equal(1, cache.Count);
	}

	#endregion
}
