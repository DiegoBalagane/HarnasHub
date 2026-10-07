#region Usings

using HarnasHub.Application.Features.Stats.GetTeamTrend;
using HarnasHub.Core.Entities;
using HarnasHub.Tests.Common;
using Xunit;

#endregion

namespace HarnasHub.Tests.Application.Features.Stats.GetTeamTrend;

public class GetTeamTrendHandlerTests
{
	#region Public Methods

	[Fact]
	public async Task Should_count_a_draw_as_a_draw_not_a_loss()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var start = new DateTime(2026, 10, 5, 18, 0, 0, DateTimeKind.Utc);
		dbContext.MatchResults.AddRange(
			Match(start, 3, 13),
			Match(start.AddDays(1), 12, 12),
			Match(start.AddDays(2), 17, 7));
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var result = await new GetTeamTrendHandler(dbContext).Handle(new GetTeamTrendQuery(), CancellationToken.None);

		var points = result.Value;
		Assert.True(points[1].Draw);
		Assert.False(points[1].Won);
		var last = points[^1];
		Assert.Equal(1, last.CumulativeWins);
		Assert.Equal(1, last.CumulativeLosses);
		Assert.Equal(1, last.CumulativeDraws);
		Assert.Equal(100.0 / 3, last.WinRatePercentage, 3);
	}

	#endregion

	#region Private Methods

	private static MatchResult Match(DateTime playedAtUtc, int ours, int theirs) =>
		new() { Id = Guid.NewGuid(), Opponent = "X", OurScore = ours, OpponentScore = theirs, PlayedAtUtc = playedAtUtc, CreatedAtUtc = playedAtUtc };

	#endregion
}
