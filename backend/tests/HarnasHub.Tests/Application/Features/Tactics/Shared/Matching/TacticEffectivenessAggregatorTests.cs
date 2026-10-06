#region Usings

using HarnasHub.Application.Features.Tactics.Shared.Matching;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Common;
using Xunit;

#endregion

namespace HarnasHub.Tests.Application.Features.Tactics.Shared.Matching;

public class TacticEffectivenessAggregatorTests
{
	#region Public Methods

	[Fact]
	public void Should_count_rounds_wins_and_matches_per_tactic_and_list_unplayed_tactics()
	{
		var played = new TacticTarget(Guid.NewGuid(), "A split", MapSide.T, [new TacticTargetPoint(0.5f, 0.5f, false, null)]);
		var unplayed = new TacticTarget(Guid.NewGuid(), "B rush", MapSide.T, [new TacticTargetPoint(0.1f, 0.1f, false, null)]);
		IReadOnlyList<RoundSignature> first = [Round(1, true), Round(2, false), Round(3, true, far: true)];
		IReadOnlyList<RoundSignature> second = [Round(1, true)];

		var report = TacticEffectivenessAggregator.Aggregate(MapName.Mirage, true, [first, second], [played, unplayed], 1);

		Assert.Equal(2, report.MatchesAnalyzed);
		Assert.Equal(1, report.MatchesSkipped);
		Assert.Equal(4, report.RoundsAnalyzed);
		Assert.Equal(3, report.RoundsMatched);
		var a = report.Tactics.Single(t => t.TacticId == played.TacticId);
		Assert.Equal((3, 2, 2), (a.RoundsPlayed, a.RoundsWon, a.Matches));
		var b = report.Tactics.Single(t => t.TacticId == unplayed.TacticId);
		Assert.Equal((0, 0, 0), (b.RoundsPlayed, b.RoundsWon, b.Matches));
	}

	[Fact]
	public async Task Should_load_points_linked_to_library_nades_as_typed_grenade_points()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var nade = new NadeEntry { Id = Guid.NewGuid(), MapName = MapName.Mirage, Type = GrenadeType.Molotov, Title = "Molo", CreatedAtUtc = DateTime.UtcNow };
		var tactic = new Tactic
		{
			Id = Guid.NewGuid(),
			MapName = MapName.Mirage,
			Side = MapSide.T,
			Name = "A exec",
			CreatedAtUtc = DateTime.UtcNow,
			Points =
			[
				new TacticPoint { Id = Guid.NewGuid(), Order = 1, X = 0.1f, Y = 0.2f, NadeEntryId = nade.Id },
				new TacticPoint { Id = Guid.NewGuid(), Order = 2, X = 0.3f, Y = 0.4f }
			]
		};
		dbContext.NadeEntries.Add(nade);
		dbContext.Tactics.Add(tactic);
		dbContext.Tactics.Add(new Tactic { Id = Guid.NewGuid(), MapName = MapName.Ancient, Side = MapSide.T, Name = "Other map", CreatedAtUtc = DateTime.UtcNow });
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var targets = await TacticEffectivenessAggregator.LoadTargetsAsync(dbContext, MapName.Mirage, CancellationToken.None);

		var target = Assert.Single(targets);
		Assert.Contains(target.Points, p => p.IsGrenade && p.GrenadeType == GrenadeType.Molotov);
		Assert.Contains(target.Points, p => !p.IsGrenade && p.GrenadeType is null);
	}

	#endregion

	#region Private Methods

	private static RoundSignature Round(int number, bool won, bool far = false) =>
		new(number, MapSide.T, won, true, [far ? new SignaturePoint(0.9f, 0.9f) : new SignaturePoint(0.5f, 0.5f)], []);

	#endregion
}
