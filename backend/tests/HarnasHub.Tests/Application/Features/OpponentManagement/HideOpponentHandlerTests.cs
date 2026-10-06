using HarnasHub.Application.Features.OpponentManagement.GetOpponentDeletePreview;
using HarnasHub.Application.Features.OpponentManagement.HideOpponent;
using HarnasHub.Application.Features.OpponentManagement.UnhideOpponent;
using HarnasHub.Application.Features.OpponentNotes.GetOpponentProfile;
using HarnasHub.Application.Features.OpponentNotes.GetOpponents;
using HarnasHub.Core.Entities;
using HarnasHub.Tests.Common;
using Xunit;
using static HarnasHub.Tests.Application.Features.OpponentNotes.OpponentTestData;

namespace HarnasHub.Tests.Application.Features.OpponentManagement;

public class HideOpponentHandlerTests
{
	#region Public Methods

	[Fact]
	public async Task Hidden_opponent_should_be_excluded_from_the_list_but_keep_its_history_and_profile()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		dbContext.MatchResults.AddRange(Match("Team X", 13, 5, DateTime.UtcNow), Match("Team Y", 13, 5, DateTime.UtcNow));
		await dbContext.SaveChangesAsync(CancellationToken.None);
		var userId = Guid.NewGuid();

		var hide = await new HideOpponentHandler(dbContext, new TestCurrentUserService(userId), new TestRealtimeNotifier())
			.Handle(new HideOpponentCommand(" TEAM X"), CancellationToken.None);

		Assert.False(hide.IsError);
		var row = Assert.Single(dbContext.HiddenOpponents);
		Assert.Equal(("team x", userId), (row.OpponentKey, row.HiddenByUserId));
		var visible = (await new GetOpponentsHandler(dbContext).Handle(new GetOpponentsQuery(), CancellationToken.None)).Value;
		Assert.Equal("Team Y", Assert.Single(visible).Name);
		var all = (await new GetOpponentsHandler(dbContext).Handle(new GetOpponentsQuery(true), CancellationToken.None)).Value;
		Assert.Equal(2, all.Count);
		Assert.True(all.Single(o => o.Name == "Team X").IsHidden);
		Assert.False(all.Single(o => o.Name == "Team Y").IsHidden);
		var profile = (await new GetOpponentProfileHandler(dbContext).Handle(new GetOpponentProfileQuery("Team X"), CancellationToken.None)).Value;
		Assert.True(profile.IsHidden);
		Assert.Single(profile.Matches);
	}

	[Fact]
	public async Task Hiding_twice_should_keep_a_single_row()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var handler = new HideOpponentHandler(dbContext, new TestCurrentUserService(Guid.NewGuid()), new TestRealtimeNotifier());

		await handler.Handle(new HideOpponentCommand("Team X"), CancellationToken.None);
		await handler.Handle(new HideOpponentCommand("team x"), CancellationToken.None);

		Assert.Single(dbContext.HiddenOpponents);
	}

	[Fact]
	public async Task Unhide_should_restore_the_opponent_in_the_list()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		dbContext.MatchResults.Add(Match("Team X", 13, 5, DateTime.UtcNow));
		dbContext.HiddenOpponents.Add(new HiddenOpponent { Id = Guid.NewGuid(), OpponentKey = "team x", DisplayName = "Team X" });
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var result = await new UnhideOpponentHandler(dbContext, new TestRealtimeNotifier()).Handle(new UnhideOpponentCommand("TEAM X"), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Empty(dbContext.HiddenOpponents);
		var visible = (await new GetOpponentsHandler(dbContext).Handle(new GetOpponentsQuery(), CancellationToken.None)).Value;
		Assert.Single(visible);
	}

	[Fact]
	public async Task Hidden_opponent_without_history_should_only_appear_with_include_hidden()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		dbContext.HiddenOpponents.Add(new HiddenOpponent { Id = Guid.NewGuid(), OpponentKey = "ghost", DisplayName = "Ghost" });
		await dbContext.SaveChangesAsync(CancellationToken.None);
		var handler = new GetOpponentsHandler(dbContext);

		Assert.Empty((await handler.Handle(new GetOpponentsQuery(), CancellationToken.None)).Value);
		var all = (await handler.Handle(new GetOpponentsQuery(true), CancellationToken.None)).Value;
		var ghost = Assert.Single(all);
		Assert.Equal(("Ghost", true), (ghost.Name, ghost.IsHidden));
	}

	[Fact]
	public async Task Delete_preview_should_count_everything_under_the_key()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		dbContext.OpponentNotes.Add(Note("Team X", DateTime.UtcNow));
		dbContext.MatchResults.AddRange(Match("team x", 1, 0, DateTime.UtcNow), Match("TEAM X", 1, 0, DateTime.UtcNow), Match("Other", 1, 0, DateTime.UtcNow));
		dbContext.Events.Add(Game("Team X", DateTime.UtcNow));
		dbContext.OpponentFaceitLinks.Add(new OpponentFaceitLink { Id = Guid.NewGuid(), OpponentKey = "team x" });
		dbContext.OpponentDemoAnalyses.Add(new OpponentDemoAnalysis { Id = Guid.NewGuid(), OpponentKey = "team x" });
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var result = await new GetOpponentDeletePreviewHandler(dbContext).Handle(new GetOpponentDeletePreviewQuery("Team X"), CancellationToken.None);

		var preview = result.Value;
		Assert.Equal((1, 1, true, false, 2, 1, false),
			(preview.Notes, preview.DemoAnalyses, preview.HasFaceitLink, preview.HasReportSnapshot, preview.MatchResults, preview.Events, preview.IsHidden));
	}

	[Fact]
	public void Validators_should_require_a_name()
	{
		Assert.False(new HideOpponentCommandValidator().Validate(new HideOpponentCommand("")).IsValid);
		Assert.False(new UnhideOpponentCommandValidator().Validate(new UnhideOpponentCommand(new string('a', 101))).IsValid);
		Assert.True(new GetOpponentDeletePreviewQueryValidator().Validate(new GetOpponentDeletePreviewQuery("Team")).IsValid);
	}

	#endregion
}
