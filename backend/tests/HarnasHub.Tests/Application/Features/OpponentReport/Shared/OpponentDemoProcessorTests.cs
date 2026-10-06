#region Usings

using HarnasHub.Application.Features.OpponentReport.Shared;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Common;
using Xunit;
using static HarnasHub.Tests.Application.Features.OpponentReport.OpponentTimelineFactory;

#endregion

namespace HarnasHub.Tests.Application.Features.OpponentReport.Shared;

public class OpponentDemoProcessorTests
{
	#region Public Methods

	[Theory]
	[InlineData("team x", "team-x")]
	[InlineData("  Ęlo / Ziomki!! ", "lo-ziomki")]
	[InlineData("???", "opponent")]
	public void Should_slugify_the_opponent_key(string key, string expected)
	{
		Assert.Equal(expected, OpponentDemoProcessor.Slug(key));
	}

	[Fact]
	public void Should_build_the_timeline_key_under_the_opponents_prefix()
	{
		var id = Guid.NewGuid();

		Assert.Equal($"opponents/team-x/{id:N}.json.gz", OpponentDemoProcessor.TimelineKey("team x", id));
	}

	[Fact]
	public async Task Should_detect_the_opponent_from_their_faceit_players_and_store_the_timeline()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		OpponentDemoRows.SeedLink(dbContext);
		var storage = new TestFileStorage();
		var timeline = Timeline([Round(1, MapSide.T, MapSide.T), Round(2, MapSide.T, MapSide.CT)]);

		var analysis = await OpponentDemoProcessor.StoreAsync(dbContext, storage,
			new OpponentDemoInput("team x", OpponentDemoSource.Upload, null, null, null, null), timeline, DateTime.UtcNow, CancellationToken.None);
		await dbContext.SaveChangesAsync(CancellationToken.None);

		Assert.Equal(Them, analysis.OpponentSteamIds);
		Assert.NotNull(analysis.FactsJson);
		Assert.Equal(MapName.Mirage, analysis.MapName);
		Assert.Equal(2, analysis.RoundsCount);
		Assert.True(storage.Objects.ContainsKey(analysis.TimelineObjectKey));
		var dto = OpponentDemoProcessor.ToDto(analysis);
		Assert.True(dto.TeamResolved);
		Assert.Equal("A", dto.OpponentTeam);
		Assert.Equal(2, dto.Teams.Count);
	}

	[Fact]
	public async Task Should_take_the_team_opposite_our_roster_when_the_opponent_is_unknown()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		dbContext.Users.Add(new User { Id = Guid.NewGuid(), DiscordId = "d1", DisplayName = "me", SteamId64 = "21" });
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var team = await OpponentDemoProcessor.DetectTeamAsync(dbContext, "team x", Timeline([Round(1, MapSide.T, null)]), CancellationToken.None);

		Assert.Equal("A", team);
	}

	[Fact]
	public async Task Should_leave_the_team_unresolved_when_nothing_identifies_it()
	{
		await using var dbContext = TestApplicationDbContext.Create();

		var analysis = await OpponentDemoProcessor.StoreAsync(dbContext, new TestFileStorage(),
			new OpponentDemoInput("team x", OpponentDemoSource.Upload, null, null, null, null),
			Timeline([Round(1, MapSide.T, null)]), DateTime.UtcNow, CancellationToken.None);

		Assert.Empty(analysis.OpponentSteamIds);
		Assert.Null(analysis.FactsJson);
		Assert.False(OpponentDemoProcessor.ToDto(analysis).TeamResolved);
	}

	#endregion
}
