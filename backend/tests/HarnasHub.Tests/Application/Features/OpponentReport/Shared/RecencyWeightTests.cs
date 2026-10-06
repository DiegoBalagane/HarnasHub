using HarnasHub.Application.Features.OpponentReport.Shared;
using HarnasHub.Core.Enums;
using Xunit;
using static HarnasHub.Tests.Application.Features.OpponentReport.FaceitTestData;

namespace HarnasHub.Tests.Application.Features.OpponentReport.Shared;

public class RecencyWeightTests
{
	#region Private Fields

	private static readonly DateTime Now = new(2026, 10, 1, 12, 0, 0, DateTimeKind.Utc);

	#endregion

	#region Public Methods

	[Fact]
	public void Should_halve_the_weight_every_half_life()
	{
		Assert.Equal(1, RecencyWeight.Of(Now, Now));
		Assert.Equal(0.5, RecencyWeight.Of(Now.AddDays(-RecencyWeight.HalfLifeDays), Now), 6);
		Assert.Equal(0.25, RecencyWeight.Of(Now.AddDays(-2 * RecencyWeight.HalfLifeDays), Now), 6);
		Assert.Equal(1, RecencyWeight.Of(Now.AddDays(-300), null));
	}

	[Fact]
	public void Should_let_recent_losses_outweigh_old_wins_while_keeping_raw_counts()
	{
		// Mirage: 6 wins ~100 days ago, 4 losses in the last week; Inferno played only recently.
		var matches = Enumerable.Range(0, 6).Select(i => Match("de_mirage", Them, Strangers(), 13, 5, Now.AddDays(-100 - i)))
			.Concat(Enumerable.Range(0, 4).Select(i => Match("de_mirage", Them, Strangers(), 5, 13, Now.AddDays(-i - 1))))
			.Concat(Enumerable.Range(0, 4).Select(i => Match("de_inferno", Them, Strangers(), 13, 10, Now.AddDays(-i - 1))))
			.ToList();
		var games = TeamMatchDetector.Detect(matches, Them.ToHashSet());

		var weighted = MapMetricsCalculator.Calculate(games, Now);
		var flat = MapMetricsCalculator.Calculate(games);

		var mirage = weighted[MapName.Mirage];
		Assert.Equal((10, 6, 0.6), (mirage.Games, mirage.Wins, mirage.WinRate!.Value));
		Assert.True(mirage.SmoothedWinRate < 0.5);
		Assert.True(flat[MapName.Mirage].SmoothedWinRate > 0.5);
		Assert.True(weighted[MapName.Inferno].DecisionShare > weighted[MapName.Inferno].Share);
		Assert.True(OpponentVetoPredictor.Preference(weighted[MapName.Inferno]) > OpponentVetoPredictor.Preference(mirage));
	}

	#endregion
}
