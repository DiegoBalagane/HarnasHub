#region Usings

using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.MatchAnalysis.Shared.Analysis;
using HarnasHub.Application.Features.MatchAnalysis.Shared.Replay;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Application.Features.Tactics;
using Xunit;
using static HarnasHub.Tests.Application.Features.MatchAnalysis.MatchTimelineFactory;

#endregion

namespace HarnasHub.Tests.Application.Features.MatchAnalysis.Shared.Analysis;

public class RoundParticipantsTests
{
	#region Private Fields

	private const long Coach = 99;

	#endregion

	#region Public Methods

	[Fact]
	public void Should_drop_a_roster_member_who_never_appears_in_the_match()
	{
		var timeline = WithCoach([Kill(1, 3, 1, seconds: 10), Kill(1, 4, 2, seconds: 20)]);

		var round = Assert.Single(RoundParticipants.Filter(timeline).Rounds);

		Assert.DoesNotContain(Coach, round.TerroristSteamIds);
		Assert.Equal([1L, 2L], round.TerroristSteamIds);
	}

	[Fact]
	public void Should_use_position_tracks_of_the_round_when_present()
	{
		var tracks = new[] { 1L, 2, 3, 4 }.Select(id => new DemoPlayerTrack(1, id, MapSide.T, 0, [0, 0, 0], null, [100], [""])).ToList();
		var timeline = WithCoach([]) with { Positions = tracks };

		Assert.DoesNotContain(Coach, RoundParticipants.Filter(timeline).Rounds[0].TerroristSteamIds);
	}

	[Fact]
	public void Should_not_invent_a_clutch_for_a_non_playing_roster_member()
	{
		var timeline = WithCoach([Kill(1, 3, 1, seconds: 10), Kill(1, 4, 2, seconds: 20)]);

		var result = ClutchAnalyzer.Analyze(AnalysisContext.Create(timeline, DemoTimelineFactory.TeamA));

		Assert.DoesNotContain(result.Clutches, c => c.SteamId64 == Coach.ToString());
		Assert.Equal("2", Assert.Single(result.Clutches, c => c.IsOurs).SteamId64);
	}

	[Fact]
	public void Should_keep_the_coach_out_of_the_replay_players()
	{
		var timeline = WithCoach([Kill(1, 3, 1), Kill(1, 4, 2)]);

		var replay = RoundReplayMapper.Build(timeline, 1, new ReplayPerspective(DemoTimelineFactory.TeamA, DemoTimelineFactory.TeamB, "x"));

		Assert.NotNull(replay);
		Assert.DoesNotContain(replay.Players, p => p.SteamId64 == Coach.ToString());
	}

	[Fact]
	public void Should_label_an_unknown_player_with_the_fallback_instead_of_the_raw_steam_id()
	{
		var context = AnalyzerTestSupport.Context([Round(1, MapSide.T)]);

		Assert.Equal("Gracz …0123", context.Name(76561198210690123));
	}

	[Fact]
	public void Should_treat_an_excluded_player_as_not_having_played()
	{
		var timeline = Timeline([Round(1, MapSide.T)], null, [Kill(1, 3, 1, seconds: 10), Kill(1, 4, 2, seconds: 20)]);

		var result = RoundParticipants.Exclude(timeline, [1]);

		Assert.Equal([2L], result.Rounds[0].TerroristSteamIds);
		Assert.DoesNotContain(result.Kills, k => k.Victim.SteamId64 == 1);
		Assert.Same(timeline, RoundParticipants.Exclude(timeline, []));
	}

	#endregion

	#region Private Methods

	private static DemoTimeline WithCoach(IReadOnlyList<DemoKill> kills)
	{
		var round = Round(1, MapSide.T) with { TerroristSteamIds = [1, 2, Coach] };
		return Timeline([round], null, kills);
	}

	#endregion
}
