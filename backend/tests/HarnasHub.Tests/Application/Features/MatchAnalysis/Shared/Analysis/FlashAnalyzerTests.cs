#region Usings

using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.MatchAnalysis.Shared.Analysis;
using HarnasHub.Core.Enums;
using Xunit;
using static HarnasHub.Tests.Application.Features.MatchAnalysis.MatchTimelineFactory;

#endregion

namespace HarnasHub.Tests.Application.Features.MatchAnalysis.Shared.Analysis;

public class FlashAnalyzerTests
{
	#region Public Methods

	[Fact]
	public void Should_report_no_data_without_blind_events()
	{
		var result = FlashAnalyzer.Analyze(AnalyzerTestSupport.Context([Round(1, MapSide.T)]));

		Assert.False(result.HasData);
	}

	[Fact]
	public void Should_split_enemy_and_team_flashes_and_ignore_self_flashes_and_glances()
	{
		var blinds = new[]
		{
			Blind(1, 3, 2f, false),
			Blind(1, 4, 4f, false),
			Blind(1, 2, 1.5f, true),
			Blind(1, 1, 3f, true),
			Blind(1, 3, 0.2f, false)
		};

		var result = FlashAnalyzer.Analyze(AnalyzerTestSupport.Context([Round(1, MapSide.T)], blinds: blinds));

		var player = Assert.Single(result.Players);
		Assert.Equal(2, player.EnemiesFlashed);
		Assert.Equal(3.0, player.AvgEnemyBlindSeconds);
		Assert.Equal(1, player.TeamFlashes);
		Assert.Equal(1.5, player.TeamBlindSeconds);
	}

	#endregion

	#region Private Methods

	private static DemoBlind Blind(long attacker, long victim, float seconds, bool team) => new(
		1, 5f,
		new DemoKillParticipant(attacker, $"p{attacker}", MapSide.T, null),
		new DemoKillParticipant(victim, $"p{victim}", team ? MapSide.T : MapSide.CT, null),
		seconds, team);

	#endregion
}
