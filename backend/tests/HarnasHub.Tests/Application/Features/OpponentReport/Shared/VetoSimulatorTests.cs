using HarnasHub.Application.Features.OpponentReport.Shared;
using HarnasHub.Application.Features.Veto.Shared;
using HarnasHub.Core.Enums;
using Xunit;

namespace HarnasHub.Tests.Application.Features.OpponentReport.Shared;

public class VetoSimulatorTests
{
	#region Private Fields

	// Our scores: Mirage best … Dust2 worst. Their preference: Ancient favourite, Nuke and Cache never played.
	private static readonly List<VetoCandidate> Maps =
	[
		Candidate(MapName.Mirage, 40, 0.9),
		Candidate(MapName.Inferno, 30, 0.6),
		Candidate(MapName.Ancient, 20, 1.4),
		Candidate(MapName.Anubis, 10, 0.5),
		Candidate(MapName.Nuke, 0, 0),
		Candidate(MapName.Cache, -10, 0),
		Candidate(MapName.Dust2, -30, 0.7)
	];

	#endregion

	#region Public Methods

	[Fact]
	public void Should_alternate_bans_down_to_one_map_in_bo1()
	{
		var steps = VetoSimulator.Simulate(Maps, VetoFormat.Bo1);

		Assert.Equal(7, steps.Count);
		Assert.Equal(["Us", "Opponent", "Us", "Opponent", "Us", "Opponent", "Us"], steps.Select(s => s.Actor));
		Assert.All(steps.Take(6), s => Assert.Equal("Ban", s.Action));
		Assert.Equal("Decider", steps[^1].Action);
		Assert.Equal(Enum.GetValues<MapName>().Length, steps.Select(s => s.MapName).Distinct().Count());
	}

	[Fact]
	public void Should_ban_our_worst_map_first_and_predict_their_ban_on_an_unplayed_map()
	{
		var steps = VetoSimulator.Simulate(Maps, VetoFormat.Bo1);

		Assert.Equal("Dust2", steps[0].MapName);
		Assert.Contains("Najsłabsza dla nas", steps[0].Reason);
		// Nuke and Cache are both unplayed; they ban the one that is better for us first.
		Assert.Equal("Nuke", steps[1].MapName);
		Assert.Contains("Przewidywany ban rywala", steps[1].Reason);
	}

	[Fact]
	public void Should_follow_ban_ban_pick_pick_ban_ban_decider_in_bo3()
	{
		var steps = VetoSimulator.Simulate(Maps, VetoFormat.Bo3);

		Assert.Equal(["Ban", "Ban", "Pick", "Pick", "Ban", "Ban", "Decider"], steps.Select(s => s.Action));
		Assert.Equal("Mirage", steps[2].MapName);
		Assert.Equal("Us", steps[2].Actor);
		Assert.Equal("Ancient", steps[3].MapName);
		Assert.Equal("Opponent", steps[3].Actor);
	}

	[Fact]
	public void Should_let_the_opponent_start_when_asked()
	{
		var steps = VetoSimulator.Simulate(Maps, VetoFormat.Bo1, weStart: false);

		Assert.Equal("Opponent", steps[0].Actor);
	}

	[Fact]
	public void Should_handle_a_smaller_pool()
	{
		var steps = VetoSimulator.Simulate(Maps.Take(3).ToList(), VetoFormat.Bo3);

		Assert.Equal(["Ban", "Ban", "Decider"], steps.Select(s => s.Action));
	}

	[Fact]
	public void Should_ban_maps_we_dont_play_before_a_known_map_with_a_lower_score()
	{
		List<VetoCandidate> maps =
		[
			Candidate(MapName.Mirage, 40, 0.9),
			Candidate(MapName.Ancient, -11, 1.0) with { OurNote = "znamy mapę (6 meczów)", OurTier = VetoBanTier.BanCandidate },
			Candidate(MapName.Dust2, 0, 0.5) with { OurNote = "nie gramy tej mapy (1 mecz) — ban w pierwszej kolejności", OurTier = VetoBanTier.NotPlayed }
		];

		var steps = VetoSimulator.Simulate(maps, VetoFormat.Bo1);

		Assert.Equal("Dust2", steps[0].MapName);
		Assert.StartsWith("Nie gramy tej mapy (1 mecz) — ban w pierwszej kolejności;", steps[0].Reason);
	}

	[Fact]
	public void Should_explain_a_known_map_ban_once_the_unplayed_maps_are_gone()
	{
		List<VetoCandidate> maps =
		[
			Candidate(MapName.Mirage, 40, 0.1),
			Candidate(MapName.Ancient, -11, 1.0) with { OurTier = VetoBanTier.BanCandidate },
			Candidate(MapName.Dust2, 0, 0.0) with { OurTier = VetoBanTier.NotPlayed },
			Candidate(MapName.Nuke, 20, 0.8)
		];

		// We ban Dust2, they ban Mirage (their least played), we ban Ancient.
		var steps = VetoSimulator.Simulate(maps, VetoFormat.Bo1);

		Assert.Equal(["Dust2", "Mirage", "Ancient", "Nuke"], steps.Select(s => s.MapName));
		Assert.StartsWith("Mapy, których nie gramy, już odpadły — najsłabsza z pozostałych:", steps[2].Reason);
	}

	[Fact]
	public void Should_pick_a_known_map_over_one_we_dont_play()
	{
		List<VetoCandidate> maps =
		[
			Candidate(MapName.Mirage, 5, 0.9),
			Candidate(MapName.Dust2, 20, 1.5) with { OurTier = VetoBanTier.NotPlayed },
			Candidate(MapName.Cache, -30, 0.5) with { OurTier = VetoBanTier.NotPlayed },
			Candidate(MapName.Nuke, 0, 0.2),
			Candidate(MapName.Inferno, -5, 0.3),
			Candidate(MapName.Ancient, -10, 0.4)
		];

		var steps = VetoSimulator.Simulate(maps, VetoFormat.Bo3);

		// We ban Cache, they ban Nuke; our pick skips the higher-scored Dust2 we don't play.
		Assert.Equal(["Cache", "Nuke", "Mirage"], steps.Take(3).Select(s => s.MapName));
		Assert.Equal("Pick", steps[2].Action);
	}

	#endregion

	#region Private Methods

	private static VetoCandidate Candidate(MapName map, int ourScore, double theirPreference) =>
		new(map, ourScore, theirPreference, "my: 3 mecze, 66% wygranych", "oni: 2 mecze, 50% wygranych");

	#endregion
}
