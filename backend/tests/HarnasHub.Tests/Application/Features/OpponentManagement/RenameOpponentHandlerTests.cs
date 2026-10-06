using HarnasHub.Application.Features.OpponentManagement.RenameOpponent;
using HarnasHub.Core.Entities;
using HarnasHub.Tests.Common;
using Xunit;
using static HarnasHub.Tests.Application.Features.OpponentNotes.OpponentTestData;

namespace HarnasHub.Tests.Application.Features.OpponentManagement;

public class RenameOpponentHandlerTests
{
	#region Public Methods

	[Fact]
	public async Task Should_merge_into_an_existing_opponent_and_rewrite_names_everywhere()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		dbContext.OpponentNotes.Add(Note("TEAM TEST", DateTime.UtcNow));
		dbContext.MatchResults.Add(Match("TEAM TEST ", 13, 5, DateTime.UtcNow));
		dbContext.Events.Add(Game("TEAM TEST", DateTime.UtcNow.AddDays(1)));
		dbContext.MatchResults.Add(Match("team TEST2", 5, 13, DateTime.UtcNow));
		dbContext.OpponentDemoAnalyses.Add(new OpponentDemoAnalysis { Id = Guid.NewGuid(), OpponentKey = "team test", TimelineObjectKey = "k" });
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var result = await new RenameOpponentHandler(dbContext, new TestRealtimeNotifier())
			.Handle(new RenameOpponentCommand("TEAM TEST", "  Team Test2 "), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Equal("Team Test2", result.Value.Name);
		Assert.Equal((1, 1, 1), (result.Value.UpdatedNotes, result.Value.UpdatedMatchResults, result.Value.UpdatedEvents));
		Assert.All(dbContext.OpponentNotes, n => Assert.Equal("Team Test2", n.OpponentName));
		Assert.Equal(1, dbContext.MatchResults.Count(m => m.Opponent == "Team Test2"));
		Assert.Equal(1, dbContext.MatchResults.Count(m => m.Opponent == "team TEST2"));
		Assert.All(dbContext.Events, e => Assert.Equal("Team Test2", e.Opponent));
		Assert.Equal("team test2", Assert.Single(dbContext.OpponentDemoAnalyses).OpponentKey);
	}

	[Fact]
	public async Task Should_move_faceit_link_and_snapshot_when_the_target_has_none()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		dbContext.OpponentNotes.Add(Note("Typo", DateTime.UtcNow));
		dbContext.OpponentFaceitLinks.Add(new OpponentFaceitLink { Id = Guid.NewGuid(), OpponentKey = "typo", DisplayName = "Typo", PlayerIds = ["p1"] });
		dbContext.OpponentReportSnapshots.Add(new OpponentReportSnapshot { Id = Guid.NewGuid(), OpponentKey = "typo" });
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var result = await new RenameOpponentHandler(dbContext, new TestRealtimeNotifier())
			.Handle(new RenameOpponentCommand("Typo", "Correct"), CancellationToken.None);

		Assert.False(result.Value.FaceitDataKept);
		var link = Assert.Single(dbContext.OpponentFaceitLinks);
		Assert.Equal(("correct", "Correct"), (link.OpponentKey, link.DisplayName));
		Assert.Equal("correct", Assert.Single(dbContext.OpponentReportSnapshots).OpponentKey);
	}

	[Fact]
	public async Task Should_keep_the_targets_faceit_data_when_both_have_one()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		dbContext.OpponentNotes.Add(Note("Typo", DateTime.UtcNow));
		dbContext.OpponentFaceitLinks.AddRange(
			new OpponentFaceitLink { Id = Guid.NewGuid(), OpponentKey = "typo", DisplayName = "Typo", PlayerIds = ["old"] },
			new OpponentFaceitLink { Id = Guid.NewGuid(), OpponentKey = "correct", DisplayName = "Correct", PlayerIds = ["keep"] });
		dbContext.OpponentReportSnapshots.AddRange(
			new OpponentReportSnapshot { Id = Guid.NewGuid(), OpponentKey = "typo", ReportJson = "{\"a\":1}" },
			new OpponentReportSnapshot { Id = Guid.NewGuid(), OpponentKey = "correct", ReportJson = "{\"b\":2}" });
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var result = await new RenameOpponentHandler(dbContext, new TestRealtimeNotifier())
			.Handle(new RenameOpponentCommand("Typo", "Correct"), CancellationToken.None);

		Assert.True(result.Value.FaceitDataKept);
		Assert.Equal(["keep"], Assert.Single(dbContext.OpponentFaceitLinks).PlayerIds);
		Assert.Equal("{\"b\":2}", Assert.Single(dbContext.OpponentReportSnapshots).ReportJson);
	}

	[Fact]
	public async Task Should_only_change_the_spelling_when_the_key_is_the_same()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		dbContext.MatchResults.Add(Match("team x", 13, 5, DateTime.UtcNow));
		dbContext.OpponentFaceitLinks.Add(new OpponentFaceitLink { Id = Guid.NewGuid(), OpponentKey = "team x", DisplayName = "team x" });
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var result = await new RenameOpponentHandler(dbContext, new TestRealtimeNotifier())
			.Handle(new RenameOpponentCommand("team x", "Team X"), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.False(result.Value.FaceitDataKept);
		Assert.Equal("Team X", Assert.Single(dbContext.MatchResults).Opponent);
		var link = Assert.Single(dbContext.OpponentFaceitLinks);
		Assert.Equal(("team x", "Team X"), (link.OpponentKey, link.DisplayName));
	}

	[Fact]
	public async Task Should_carry_the_hidden_flag_to_the_new_name()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		dbContext.OpponentNotes.Add(Note("Old", DateTime.UtcNow));
		dbContext.HiddenOpponents.Add(new HiddenOpponent { Id = Guid.NewGuid(), OpponentKey = "old", DisplayName = "Old" });
		await dbContext.SaveChangesAsync(CancellationToken.None);

		await new RenameOpponentHandler(dbContext, new TestRealtimeNotifier())
			.Handle(new RenameOpponentCommand("Old", "New"), CancellationToken.None);

		Assert.Equal("new", Assert.Single(dbContext.HiddenOpponents).OpponentKey);
	}

	[Fact]
	public async Task Should_return_not_found_for_an_unknown_opponent()
	{
		await using var dbContext = TestApplicationDbContext.Create();

		var result = await new RenameOpponentHandler(dbContext, new TestRealtimeNotifier())
			.Handle(new RenameOpponentCommand("Nobody", "Somebody"), CancellationToken.None);

		Assert.Equal("OpponentManagement.NotFound", result.FirstError.Code);
	}

	[Theory]
	[InlineData("Team", "", false)]
	[InlineData("Team", "   ", false)]
	[InlineData("", "Team", false)]
	[InlineData("Team", "Team 2", true)]
	public void Validator_should_require_both_names(string from, string to, bool valid) =>
		Assert.Equal(valid, new RenameOpponentCommandValidator().Validate(new RenameOpponentCommand(from, to)).IsValid);

	[Fact]
	public void Validator_should_cap_the_new_name_at_100_characters()
	{
		var validator = new RenameOpponentCommandValidator();

		Assert.True(validator.Validate(new RenameOpponentCommand("Team", new string('a', 100))).IsValid);
		Assert.False(validator.Validate(new RenameOpponentCommand("Team", new string('a', 101))).IsValid);
	}

	#endregion
}
