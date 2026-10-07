#region Usings

using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.MatchAnalysis.Shared;
using HarnasHub.Application.Features.MatchAnalysis.Shared.Analysis;
using HarnasHub.Core.Enums;

#endregion

namespace HarnasHub.Application.Features.OpponentReport.Tendencies;

/// <summary>Pure extraction of <see cref="OpponentDemoFacts"/> from a parsed timeline once the opponent's team is known.
/// Each round's side comes from the per-round majority of <c>theirTeam</c> (survives halftime and substitutes); T rounds
/// get first duel / plant / utility, CT rounds get the setup (<see cref="OpponentCtFactsExtractor"/>), and per-player
/// totals come from <see cref="OpponentPlayerFactsExtractor"/>.</summary>
public static class OpponentFactsExtractor
{
	#region Public Fields

	/// <summary>Grenades thrown after the execute + this many seconds are retakes/post-plant, not the execute's utility.</summary>
	public const float ExecGrenadeGraceSeconds = 5f;

	/// <summary>Without a plant or duel, T utility up to this second still counts as the round's default utility.</summary>
	public const float DefaultUtilityCutoffSeconds = 100f;

	#endregion

	#region Public Methods

	/// <summary>Extracts the facts; rounds the opponent didn't play (none of <paramref name="theirTeam"/> present) are skipped.</summary>
	public static OpponentDemoFacts Extract(DemoTimeline timeline, IReadOnlySet<long> theirTeam)
	{
		timeline = RoundParticipants.Filter(timeline);
		var rounds = new List<OpponentRoundFacts>();
		var killsByRound = timeline.Kills.ToLookup(k => k.RoundNumber);
		var economyByRound = timeline.Economy.ToDictionary(e => e.RoundNumber);

		foreach (var round in timeline.Rounds.OrderBy(r => r.Number))
		{
			if (TimelineTeamResolver.OurSide(round, theirTeam) is not { } side)
			{
				continue;
			}

			var roster = (side == MapSide.T ? round.TerroristSteamIds : round.CounterTerroristSteamIds).ToHashSet();
			var kills = killsByRound[round.Number].OrderBy(k => k.SecondsIntoRound).ToList();
			var won = round.WinnerSide is { } winner ? winner == side : (bool?)null;
			var buy = Buy(round.Number, economyByRound.GetValueOrDefault(round.Number), roster);

			var facts = side == MapSide.T
				? new OpponentRoundFacts(round.Number, side, won, buy, ExtractT(timeline, round, roster, kills), null)
				: new OpponentRoundFacts(round.Number, side, won, buy, null, OpponentCtFactsExtractor.Extract(timeline, round, roster, kills));
			rounds.Add(facts);
		}

		return new OpponentDemoFacts(OpponentDemoFacts.CurrentVersion, rounds, OpponentPlayerFactsExtractor.Extract(timeline, theirTeam));
	}

	/// <summary>Buy type name of the opponent's loadout in a round, or null when the round has no economy snapshot of them.</summary>
	public static string? Buy(int roundNumber, DemoRoundEconomy? economy, IReadOnlySet<long> roster)
	{
		var players = economy?.Players.Where(p => roster.Contains(p.SteamId64)).ToList();
		if (players is null || players.Count == 0)
		{
			return null;
		}

		return BuyTypeClassifier.Classify(roundNumber, players.Sum(p => p.EquipmentValue), players.Count).ToString();
	}

	#endregion

	#region Private Methods

	private static OpponentTRoundFacts ExtractT(DemoTimeline timeline, DemoTimelineRound round, IReadOnlySet<long> roster, List<DemoKill> kills)
	{
		var map = timeline.MapName;
		var first = kills.FirstOrDefault(k => !k.IsTeamKill && k.Killer is not null);

		FactPoint? contact = null;
		if (first is not null)
		{
			// Their own player's position tells where *they* went; the enemy's is only a fallback.
			var ours = roster.Contains(first.Victim.SteamId64) ? first.Victim.Position : first.Killer!.Position;
			var position = ours ?? (roster.Contains(first.Victim.SteamId64) ? first.Killer!.Position : first.Victim.Position);
			if (position is { RadarX: { } x, RadarY: { } y })
			{
				contact = new FactPoint(x, y);
			}
		}

		var plant = round.BombPlant;
		var plantArea = plant?.Site switch
		{
			DemoBombSite.A => MapArea.A,
			DemoBombSite.B => MapArea.B,
			_ => plant is null ? null : MapAreaResolver.Resolve(map, plant.Position?.RadarX, plant.Position?.RadarY)
		};

		var contactArea = contact is null ? null : MapAreaResolver.Resolve(map, contact.X, contact.Y);
		var execSecond = plant?.SecondsIntoRound ?? first?.SecondsIntoRound;
		var cutoff = execSecond is { } exec ? exec + ExecGrenadeGraceSeconds : DefaultUtilityCutoffSeconds;

		var grenades = timeline.Grenades
			.Where(g => g.RoundNumber == round.Number
				&& g.ThrowerSteamId64 is { } thrower && roster.Contains(thrower)
				&& g.Type != DemoGrenadeType.Decoy
				&& g.SecondsIntoRound <= cutoff
				&& g.Landing is { RadarX: not null, RadarY: not null })
			.Select(g => new GrenadeFact(g.Type, g.Landing!.RadarX!.Value, g.Landing.RadarY!.Value, g.SecondsIntoRound))
			.ToList();

		return new OpponentTRoundFacts(
			contactArea,
			first?.SecondsIntoRound,
			contact,
			plantArea,
			plant?.SecondsIntoRound,
			grenades);
	}

	#endregion
}
