#region Usings

using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.AnalysisBoards.Shared;
using HarnasHub.Application.Features.MatchAnalysis.Shared.Replay;
using HarnasHub.Core.Enums;
using Xunit;

#endregion

namespace HarnasHub.Tests.Application.Features.AnalysisBoards.Shared;

public class RoundBoardBuilderTests
{
	#region Public Methods

	[Fact]
	public void Should_draw_every_alive_player_as_a_closed_dot_in_the_side_colour()
	{
		var strokes = RoundBoardBuilder.Build(Replay(), 5);

		Assert.Equal(2, strokes.Count);
		Assert.Contains(strokes, s => s.Color == "#f59e0b");
		Assert.Contains(strokes, s => s.Color == "#60a5fa");
		Assert.All(strokes, s =>
		{
			Assert.Equal(s.Points[0].X, s.Points[^1].X, 3);
			Assert.Equal(s.Points[0].Y, s.Points[^1].Y, 3);
		});
	}

	[Fact]
	public void Should_draw_a_grenade_line_in_flight_and_add_the_landing_circle_after_detonation()
	{
		var replay = Replay() with { Grenades = [new ReplayGrenadeDto(1, DemoGrenadeType.Smoke, 0, MapSide.T, 4f, 6f, 24f, false, 0.9f, 0.9f, 0.5f, 0.5f)] };

		var inFlight = RoundBoardBuilder.Build(replay, 5);
		var popped = RoundBoardBuilder.Build(replay, 10);
		var gone = RoundBoardBuilder.Build(replay, 30);

		Assert.Single(inFlight, s => s.Color == "#d4d4d4");
		Assert.Equal(2, popped.Count(s => s.Color == "#d4d4d4"));
		Assert.DoesNotContain(gone, s => s.Color == "#d4d4d4");
	}

	[Fact]
	public void Should_mark_dead_players_and_the_planted_bomb()
	{
		var replay = Replay() with
		{
			Kills = [new ReplayKillDto(3f, 0, 1, "ak47", true, false, 0.3f, 0.3f)],
			Bomb = new ReplayBombDto(4f, DemoBombSite.A, 0, 0.6f, 0.6f, null, null, null)
		};

		var strokes = RoundBoardBuilder.Build(replay, 5);

		Assert.Equal(2, strokes.Count(s => s.Color == "#a3a3a3"));
		Assert.Single(strokes, s => s.Color == "#dc2626");
	}

	[Fact]
	public void Should_draw_nothing_without_available_positions()
	{
		var replay = Replay() with { PositionsStatus = ReplayPositionsStatus.UncalibratedMap };

		Assert.Empty(RoundBoardBuilder.Build(replay, 5));
	}

	[Fact]
	public void Should_title_the_board_with_round_time_and_opponent()
	{
		Assert.Equal("Runda 3 – 1:05 – vs Team X", RoundBoardBuilder.Title(Replay(), 65));
		Assert.Equal("Runda 3 – 0:09", RoundBoardBuilder.Title(Replay() with { OpponentName = null }, 9));
	}

	[Fact]
	public void Should_serialize_strokes_in_the_canvas_format()
	{
		var json = RoundBoardBuilder.ToJson([new BoardStroke("#fff", 2, [new BoardPoint(0.1f, 0.2f)])]);

		Assert.Equal("[{\"color\":\"#fff\",\"width\":2,\"points\":[{\"x\":0.1,\"y\":0.2}]}]", json);
	}

	#endregion

	#region Private Methods

	private static RoundReplayDto Replay()
	{
		ReplayPlayerDto[] players = [new(0, "1", "t1", MapSide.T, ReplayTeam.Ours), new(1, "2", "ct1", MapSide.CT, ReplayTeam.Opponent)];
		var frames = Enumerable.Range(0, 11)
			.Select(s => new ReplayFrameDto(s, [new ReplayPlayerStateDto(0, 0.2f, 0.2f, 100, "ak47"), new ReplayPlayerStateDto(1, 0.7f, 0.7f, 100, "m4a1")]))
			.ToList();
		return new RoundReplayDto("Mirage", 3, 24, "Team X", ReplayPositionsStatus.Available, 10, MapSide.T, DemoRoundEndReason.Elimination, MapSide.T,
			players, frames, [], [], null);
	}

	#endregion
}
