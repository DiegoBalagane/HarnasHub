using HarnasHub.Application.Features.OpponentNotes.GetOpponents;
using HarnasHub.Tests.Common;
using Xunit;
using static HarnasHub.Tests.Application.Features.OpponentNotes.OpponentTestData;

namespace HarnasHub.Tests.Application.Features.OpponentNotes.GetOpponents;

public class GetOpponentsHandlerTests
{
	#region Public Methods

	[Fact]
	public async Task Should_merge_spellings_case_and_whitespace_insensitively_and_count_the_record()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var now = DateTime.UtcNow;
		dbContext.OpponentNotes.Add(Note("team x", now.AddDays(-10)));
		dbContext.MatchResults.AddRange(
			Match("Team X ", 13, 7, now.AddDays(-5)),
			Match("TEAM X", 10, 13, now.AddDays(-3)),
			Match("Team X", 12, 12, now.AddDays(-1)));
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var result = await new GetOpponentsHandler(dbContext).Handle(new GetOpponentsQuery(), CancellationToken.None);

		var opponent = Assert.Single(result.Value);
		Assert.Equal("Team X", opponent.Name);
		Assert.Equal(1, opponent.NoteCount);
		Assert.Equal((1, 1, 1), (opponent.Wins, opponent.Losses, opponent.Draws));
		Assert.Equal(now.AddDays(-1), opponent.LastPlayedAtUtc);
		Assert.Null(opponent.NextEventAtUtc);
	}

	[Fact]
	public async Task Should_list_opponents_with_an_upcoming_game_first()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var now = DateTime.UtcNow;
		dbContext.MatchResults.Add(Match("Old Rival", 13, 2, now.AddDays(-1)));
		dbContext.Events.AddRange(
			Game("Next Week", now.AddDays(7)),
			Game("Tomorrow", now.AddDays(1)),
			Game("Tomorrow", now.AddDays(-20)),
			Game(null, now.AddDays(2)));
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var result = await new GetOpponentsHandler(dbContext).Handle(new GetOpponentsQuery(), CancellationToken.None);

		Assert.Equal(["Tomorrow", "Next Week", "Old Rival"], result.Value.Select(o => o.Name));
		Assert.Equal(now.AddDays(1), result.Value[0].NextEventAtUtc);
	}

	[Fact]
	public async Task Should_return_an_empty_list_without_any_data()
	{
		await using var dbContext = TestApplicationDbContext.Create();

		var result = await new GetOpponentsHandler(dbContext).Handle(new GetOpponentsQuery(), CancellationToken.None);

		Assert.Empty(result.Value);
	}

	#endregion
}
