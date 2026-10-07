using HarnasHub.Application.Features.OpponentNotes.GetOpponentProfile;
using HarnasHub.Tests.Common;
using Xunit;
using static HarnasHub.Tests.Application.Features.OpponentNotes.OpponentTestData;

namespace HarnasHub.Tests.Application.Features.OpponentNotes.GetOpponentProfile;

public class GetOpponentProfileHandlerTests
{
	#region Public Methods

	[Fact]
	public async Task Should_collect_notes_matches_maps_and_upcoming_games_for_the_opponent_only()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var now = DateTime.UtcNow;
		dbContext.OpponentNotes.AddRange(
			Note("team x", now.AddDays(-10), "Grają agresywnie"),
			Note("Someone Else", now.AddDays(-2)));
		dbContext.MatchResults.AddRange(
			Match("Team X", 13, 7, now.AddDays(-6), "Mirage"),
			Match("Team X", 13, 11, now.AddDays(-4), "Mirage"),
			Match("TEAM X", 5, 13, now.AddDays(-2), "Nuke"),
			Match("Team X", 13, 13, now.AddDays(-1)),
			Match("Someone Else", 13, 0, now.AddDays(-1), "Mirage"));
		dbContext.Events.AddRange(
			Game("Team X", now.AddDays(3)),
			Game("Team X", now.AddDays(-30)));
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var result = await new GetOpponentProfileHandler(dbContext).Handle(new GetOpponentProfileQuery(" TEAM x "), CancellationToken.None);

		var profile = result.Value;
		Assert.Equal("Team X", profile.Name);
		Assert.Equal((2, 1, 1), (profile.Wins, profile.Losses, profile.Draws));
		Assert.Equal("Grają agresywnie", Assert.Single(profile.Notes).Content);
		Assert.Equal(4, profile.Matches.Count);
		Assert.Equal(now.AddDays(-1), profile.Matches[0].PlayedAtUtc);
		Assert.Single(profile.UpcomingEvents);

		Assert.Equal(2, profile.Maps.Count);
		Assert.Equal(("Mirage", 2, 0, 0), (profile.Maps[0].MapName, profile.Maps[0].Wins, profile.Maps[0].Losses, profile.Maps[0].Draws));
		Assert.Equal(("Nuke", 0, 1, 0), (profile.Maps[1].MapName, profile.Maps[1].Wins, profile.Maps[1].Losses, profile.Maps[1].Draws));
	}

	[Fact]
	public async Task Should_use_the_faceit_link_display_name_for_an_opponent_without_other_data()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		dbContext.OpponentFaceitLinks.Add(new HarnasHub.Core.Entities.OpponentFaceitLink { Id = Guid.NewGuid(), OpponentKey = "team x", DisplayName = "Team X" });
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var result = await new GetOpponentProfileHandler(dbContext).Handle(new GetOpponentProfileQuery("team x"), CancellationToken.None);

		Assert.Equal("Team X", result.Value.Name);
	}

	[Fact]
	public async Task Should_return_an_empty_profile_for_an_opponent_without_data()
	{
		await using var dbContext = TestApplicationDbContext.Create();

		var result = await new GetOpponentProfileHandler(dbContext).Handle(new GetOpponentProfileQuery(" New Team "), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Equal("New Team", result.Value.Name);
		Assert.Empty(result.Value.Matches);
		Assert.Empty(result.Value.Notes);
		Assert.Empty(result.Value.UpcomingEvents);
		Assert.Empty(result.Value.Maps);
	}

	#endregion
}
