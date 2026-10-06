using HarnasHub.Application.Features.Veto.GetVetoSuggestion;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Common;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Veto.GetVetoSuggestion;

public class GetVetoSuggestionHandlerTests
{
	#region Public Methods

	[Fact]
	public async Task Should_score_every_pool_map_even_without_any_data()
	{
		await using var dbContext = TestApplicationDbContext.Create();

		var result = await new GetVetoSuggestionHandler(dbContext).Handle(new GetVetoSuggestionQuery(" Team X "), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Equal("Team X", result.Value.OpponentName);
		Assert.Equal(Enum.GetValues<MapName>().Length, result.Value.Maps.Count);
		Assert.Empty(result.Value.OpponentTendencies);
		Assert.Equal(0, result.Value.RecordedOpponentVetoes);
	}

	[Fact]
	public async Task Should_count_only_the_opponents_own_steps_from_vetoes_against_them()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var againstThem = AddEvent(dbContext, "team x");
		var againstThemAgain = AddEvent(dbContext, "Team X");
		var againstSomeoneElse = AddEvent(dbContext, "Other");
		AddStep(dbContext, againstThem, 1, VetoActor.Opponent, VetoAction.Ban, MapName.Nuke);
		AddStep(dbContext, againstThem, 2, VetoActor.Us, VetoAction.Ban, MapName.Dust2);
		AddStep(dbContext, againstThem, 3, VetoActor.Opponent, VetoAction.Pick, MapName.Inferno);
		AddStep(dbContext, againstThemAgain, 1, VetoActor.Opponent, VetoAction.Pick, MapName.Inferno);
		AddStep(dbContext, againstSomeoneElse, 1, VetoActor.Opponent, VetoAction.Pick, MapName.Mirage);
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var result = await new GetVetoSuggestionHandler(dbContext).Handle(new GetVetoSuggestionQuery("Team X"), CancellationToken.None);

		Assert.Equal(2, result.Value.RecordedOpponentVetoes);
		Assert.Equal(2, result.Value.OpponentTendencies.Count);
		var inferno = result.Value.OpponentTendencies[0];
		Assert.Equal(("Inferno", 2, 0), (inferno.MapName, inferno.Picks, inferno.Bans));
		var nuke = result.Value.OpponentTendencies[1];
		Assert.Equal(("Nuke", 0, 1), (nuke.MapName, nuke.Picks, nuke.Bans));
	}

	[Fact]
	public async Task Should_combine_pool_status_overall_and_head_to_head_record()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		dbContext.MapPoolEntries.Add(new MapPoolEntry
		{
			Id = Guid.NewGuid(),
			MapName = MapName.Mirage,
			Status = MapPoolStatus.Core,
			UpdatedByUserId = Guid.NewGuid(),
			UpdatedAtUtc = DateTime.UtcNow
		});
		AddResult(dbContext, "Team X", "mirage", 13, 5);
		AddResult(dbContext, "Someone", "Mirage", 13, 9);
		AddResult(dbContext, "Team X", "Nuke", 3, 13);
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var result = await new GetVetoSuggestionHandler(dbContext).Handle(new GetVetoSuggestionQuery("Team X"), CancellationToken.None);

		var mirage = result.Value.Maps[0];
		Assert.Equal("Mirage", mirage.MapName);
		Assert.Equal("Pick", mirage.Recommendation);
		Assert.Contains("Pewniak w puli map", mirage.Reasons);
		Assert.Contains("Bilans ogólny 2-0 (100% wygranych)", mirage.Reasons);
		Assert.Contains("Z tym przeciwnikiem 1-0", mirage.Reasons);
		var nuke = result.Value.Maps.Single(m => m.MapName == "Nuke");
		Assert.Contains("Z tym przeciwnikiem 0-1", nuke.Reasons);
	}

	#endregion

	#region Private Methods

	private static Guid AddEvent(TestApplicationDbContext dbContext, string opponent)
	{
		var id = Guid.NewGuid();
		dbContext.Events.Add(new Event
		{
			Id = id,
			Title = "Mecz",
			Type = EventType.Match,
			StartsAtUtc = DateTime.UtcNow,
			Opponent = opponent,
			CreatedByUserId = Guid.NewGuid(),
			CreatedAtUtc = DateTime.UtcNow
		});
		return id;
	}

	private static void AddStep(TestApplicationDbContext dbContext, Guid eventId, int order, VetoActor actor, VetoAction action, MapName map) =>
		dbContext.EventVetoSteps.Add(new EventVetoStep
		{
			Id = Guid.NewGuid(),
			EventId = eventId,
			Order = order,
			Actor = actor,
			Action = action,
			MapName = map
		});

	private static void AddResult(TestApplicationDbContext dbContext, string opponent, string map, int ourScore, int opponentScore) =>
		dbContext.MatchResults.Add(new MatchResult
		{
			Id = Guid.NewGuid(),
			Opponent = opponent,
			OurScore = ourScore,
			OpponentScore = opponentScore,
			MapName = map,
			Category = MatchCategory.Scrimmage,
			PlayedAtUtc = DateTime.UtcNow,
			CreatedByUserId = Guid.NewGuid(),
			CreatedAtUtc = DateTime.UtcNow
		});

	#endregion
}
