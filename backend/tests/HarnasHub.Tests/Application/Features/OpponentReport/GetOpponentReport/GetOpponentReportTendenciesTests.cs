#region Usings

using HarnasHub.Application.Features.OpponentReport.GetOpponentReport;
using HarnasHub.Application.Features.OpponentReport.Tendencies;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Common;
using Xunit;
using static HarnasHub.Tests.Application.Features.OpponentReport.OpponentDemoRows;
using static HarnasHub.Tests.Application.Features.OpponentReport.OpponentTimelineFactory;

#endregion

namespace HarnasHub.Tests.Application.Features.OpponentReport.GetOpponentReport;

public class GetOpponentReportTendenciesTests
{
	#region Public Methods

	[Fact]
	public async Task Should_add_tendencies_per_map_from_resolved_demos_only()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var facts = Facts([TRound(2, MapArea.B, 80f), TRound(3, MapArea.B, 85f)]);
		dbContext.OpponentDemoAnalyses.AddRange(
			Row(facts),
			Row(facts),
			Row(facts, map: MapName.Ancient),
			Row(),
			Row(facts, opponentKey: "someone else"),
			Row(facts, map: null));
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var result = await new GetOpponentReportHandler(dbContext, new TestFaceitClient(isConfigured: false))
			.Handle(new GetOpponentReportQuery("Team X"), CancellationToken.None);

		Assert.Equal(2, result.Value.Tendencies.Count);
		var mirage = result.Value.Tendencies[0];
		Assert.Equal("Mirage", mirage.MapName);
		Assert.Equal(2, mirage.Demos);
		Assert.Equal(4, mirage.T.Rounds);
	}

	[Fact]
	public async Task Should_leave_tendencies_empty_without_demos()
	{
		await using var dbContext = TestApplicationDbContext.Create();

		var result = await new GetOpponentReportHandler(dbContext, new TestFaceitClient(isConfigured: false))
			.Handle(new GetOpponentReportQuery("Team X"), CancellationToken.None);

		Assert.Empty(result.Value.Tendencies);
	}

	#endregion
}
