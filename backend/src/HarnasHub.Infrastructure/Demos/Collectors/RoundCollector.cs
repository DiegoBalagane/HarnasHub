#region Usings

using DemoFile;
using DemoFile.Game.Cs;
using HarnasHub.Application.Abstractions;
using HarnasHub.Core.Enums;

#endregion

namespace HarnasHub.Infrastructure.Demos.Collectors;

/// <summary>One <see cref="DemoTimelineRound"/> per official round: number, start/freeze-end/end times, winner, end
/// reason, rosters on both sides, and the bomb plant/defuse (with A/B site when the planted C4 reported it) if any.</summary>
internal sealed class RoundCollector(CsDemoParser demo, DemoRoundClock clock) : IDemoCollector
{
	#region Private Types

	private sealed record RawBombEvent(float SecondsIntoRound, long? SteamId64, string? PlayerName, int Site, (float X, float Y, float Z)? World);

	private sealed record RawRound(
		int Number,
		float? StartTime,
		float? FreezeEndTime,
		float EndTime,
		MapSide? WinnerSide,
		DemoRoundEndReason EndReason,
		List<long> Terrorists,
		List<long> CounterTerrorists,
		RawBombEvent? Plant,
		RawBombEvent? Defuse,
		DemoBombSite? PlantSite);

	#endregion

	#region Private Fields

	private readonly List<RawRound> _rounds = [];
	private RawBombEvent? _plant;
	private RawBombEvent? _defuse;

	// Site letter reported by the planted C4 entity this round (m_nBombSite, assumed 0 = A, 1 = B).
	private DemoBombSite? _plantedSite;

	#endregion

	#region Public Methods

	/// <inheritdoc />
	public void Subscribe()
	{
		demo.Source1GameEvents.RoundAnnounceMatchStart += _ => OnMatchStart();
		demo.Source1GameEvents.RoundStart += _ => ResetBombEvents();
		demo.Source1GameEvents.BombPlanted += e => _plant ??= ToBombEvent(e.Player, e.PlayerPawn, e.Site);
		demo.Source1GameEvents.BombDefused += e => _defuse ??= ToBombEvent(e.Player, e.PlayerPawn, e.Site);
		demo.Source1GameEvents.RoundEnd += OnRoundEnd;
		demo.EntityEvents.CPlantedC4.Create += OnPlantedC4;
		demo.EntityEvents.CPlantedC4.PostUpdate += OnPlantedC4;
	}

	/// <inheritdoc />
	public void Contribute(DemoTimelineBuilder builder)
	{
		// The bomb-target entity index from bomb_planted is stable for the whole map, so a round where the planted C4
		// entity wasn't observed can still borrow the letter another round resolved for the same index.
		var siteByIndex = _rounds
			.Where(r => r is { Plant: not null, PlantSite: not null })
			.GroupBy(r => r.Plant!.Site)
			.ToDictionary(g => g.Key, g => g.First().PlantSite!.Value);

		foreach (var round in _rounds)
		{
			var site = round.PlantSite
				?? (round.Plant is { } plant && siteByIndex.TryGetValue(plant.Site, out var known) ? known : null);

			builder.Rounds.Add(new DemoTimelineRound(
				round.Number,
				round.StartTime,
				round.FreezeEndTime,
				round.EndTime,
				round.WinnerSide,
				round.EndReason,
				round.Terrorists,
				round.CounterTerrorists,
				ToPublic(round.Plant, builder, site),
				ToPublic(round.Defuse, builder, site)));
		}
	}

	#endregion

	#region Private Methods

	private void OnMatchStart()
	{
		// Same reset as the stats collector: everything before the match proper (warmup, knife) is discarded.
		_rounds.Clear();
		ResetBombEvents();
	}

	private void OnPlantedC4(CPlantedC4 bomb)
	{
		// ASSUMPTION (unverified against a real demo): m_nBombSite is 0 for A and 1 for B, as in CS:GO's planted_c4.
		// Anything else is ignored and the read side falls back to map zones on the plant position.
		_plantedSite = bomb.BombSite switch
		{
			0 => DemoBombSite.A,
			1 => DemoBombSite.B,
			_ => _plantedSite
		};
	}

	private void ResetBombEvents()
	{
		_plant = null;
		_defuse = null;
		_plantedSite = null;
	}

	private RawBombEvent? ToBombEvent(CCSPlayerController? player, CCSPlayerPawn? pawn, int site)
	{
		if (!clock.IsOfficialMatchRound)
		{
			return null;
		}

		(float, float, float)? world = pawn is null ? null : (pawn.Origin.X, pawn.Origin.Y, pawn.Origin.Z);
		return new RawBombEvent(clock.SecondsIntoRound(), player is null ? null : (long)player.SteamID, player?.PlayerName, site, world);
	}

	private void OnRoundEnd(Source1RoundEndEvent e)
	{
		if (!clock.IsOfficialMatchRound)
		{
			return;
		}

		_rounds.Add(new RawRound(
			clock.CurrentRoundNumber,
			clock.RoundStartTime,
			clock.FreezeEndTime,
			clock.Now,
			DemoTeams.WinnerSide(e.Winner),
			DemoTeams.EndReason(e.Reason),
			DemoTeams.Roster(demo, CSTeamNumber.Terrorist),
			DemoTeams.Roster(demo, CSTeamNumber.CounterTerrorist),
			_plant,
			_defuse,
			_plantedSite));
	}

	private static DemoBombEvent? ToPublic(RawBombEvent? raw, DemoTimelineBuilder builder, DemoBombSite? site) => raw is null
		? null
		: new DemoBombEvent(
			raw.SecondsIntoRound,
			raw.SteamId64,
			raw.PlayerName,
			raw.Site,
			raw.World is { } w ? builder.ToPosition(w.X, w.Y, w.Z) : null)
		{ Site = site };

	#endregion
}
