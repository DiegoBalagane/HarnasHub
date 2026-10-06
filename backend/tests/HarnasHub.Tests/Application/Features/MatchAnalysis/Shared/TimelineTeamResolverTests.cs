#region Usings

using HarnasHub.Application.Features.MatchAnalysis.Shared;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Application.Features.Tactics;
using Xunit;

#endregion

namespace HarnasHub.Tests.Application.Features.MatchAnalysis.Shared;

public class TimelineTeamResolverTests
{
	#region Public Methods

	[Fact]
	public void Should_follow_our_players_across_the_halftime_swap()
	{
		var ours = DemoTimelineFactory.TeamA.ToHashSet();

		Assert.Equal(MapSide.T, TimelineTeamResolver.OurSide(DemoTimelineFactory.Round(1, MapSide.T), ours));
		Assert.Equal(MapSide.CT, TimelineTeamResolver.OurSide(DemoTimelineFactory.Round(13, MapSide.T, teamAOnT: false), ours));
	}

	[Fact]
	public void Should_return_no_side_when_none_of_our_players_played_the_round()
	{
		Assert.Null(TimelineTeamResolver.OurSide(DemoTimelineFactory.Round(1, MapSide.T), new HashSet<long> { 99 }));
	}

	[Fact]
	public void Should_pick_the_round_one_side_overlapping_the_roster()
	{
		var rounds = new[] { DemoTimelineFactory.Round(1, MapSide.T) };

		var ours = TimelineTeamResolver.ResolveOurTeam(rounds, new HashSet<long> { 3 }, null, null);

		Assert.Equal(DemoTimelineFactory.TeamB, ours);
	}

	[Fact]
	public void Should_fall_back_to_the_side_whose_score_matches_the_recorded_result()
	{
		// Team A wins 2 of 3: only team A's would-be score is 2:1.
		var rounds = new[]
		{
			DemoTimelineFactory.Round(1, MapSide.T),
			DemoTimelineFactory.Round(2, MapSide.T),
			DemoTimelineFactory.Round(3, MapSide.CT)
		};

		Assert.Equal(DemoTimelineFactory.TeamA, TimelineTeamResolver.ResolveOurTeam(rounds, new HashSet<long>(), 2, 1));
		Assert.Equal(DemoTimelineFactory.TeamB, TimelineTeamResolver.ResolveOurTeam(rounds, new HashSet<long>(), 1, 2));
	}

	[Fact]
	public void Should_give_up_when_neither_roster_nor_score_decides()
	{
		var rounds = new[] { DemoTimelineFactory.Round(1, MapSide.T), DemoTimelineFactory.Round(2, MapSide.CT) };

		Assert.Empty(TimelineTeamResolver.ResolveOurTeam(rounds, new HashSet<long>(), 1, 1));
		Assert.Empty(TimelineTeamResolver.ResolveOurTeam(rounds, new HashSet<long>(), null, null));
		Assert.Empty(TimelineTeamResolver.ResolveOurTeam([], new HashSet<long> { 1 }, 1, 0));
	}

	#endregion
}
