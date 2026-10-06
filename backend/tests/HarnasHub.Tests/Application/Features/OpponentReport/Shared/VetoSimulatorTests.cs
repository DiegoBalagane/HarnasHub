using HarnasHub.Application.Features.OpponentReport.Shared;
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

	#endregion

	#region Private Methods

	private static VetoCandidate Candidate(MapName map, int ourScore, double theirPreference) =>
		new(map, ourScore, theirPreference, "my: 3 mecze, 66% wygranych", "oni: 2 mecze, 50% wygranych");

	#endregion
}
