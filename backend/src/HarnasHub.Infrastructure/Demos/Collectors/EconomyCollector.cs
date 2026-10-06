#region Usings

using DemoFile;
using HarnasHub.Application.Abstractions;

#endregion

namespace HarnasHub.Infrastructure.Demos.Collectors;

/// <summary>Everyone's loadout per official round: snapshotted at freeze end and refreshed at buy-time end (players can
/// keep buying for ~20 s after freeze time), keeping per player whichever snapshot carried more equipment — a late
/// buy raises the value, while a grenade already thrown or a weapon dropped to a teammate must not lower it.
/// Buy-type classification is NOT done here: it is pure Application logic (<c>BuyTypeClassifier</c>) applied when the
/// timeline is read, so thresholds can be tuned without re-parsing anything.</summary>
internal sealed class EconomyCollector(CsDemoParser demo, DemoRoundClock clock) : IDemoCollector
{
	#region Private Fields

	private readonly List<DemoRoundEconomy> _rounds = [];
	private readonly Dictionary<long, DemoPlayerEconomy> _current = [];

	#endregion

	#region Public Methods

	/// <inheritdoc />
	public void Subscribe()
	{
		demo.Source1GameEvents.RoundAnnounceMatchStart += _ => _rounds.Clear();
		demo.Source1GameEvents.RoundStart += _ => _current.Clear();
		demo.Source1GameEvents.RoundFreezeEnd += _ => Snapshot();
		demo.Source1GameEvents.BuytimeEnded += _ => Snapshot();
		demo.Source1GameEvents.RoundEnd += _ => OnRoundEnd();
	}

	/// <inheritdoc />
	public void Contribute(DemoTimelineBuilder builder) => builder.Economy.AddRange(_rounds);

	#endregion

	#region Private Methods

	private void Snapshot()
	{
		// Deliberately not gated on IsOfficialMatchRound: in round 1 round_announce_match_start can fire after
		// round_freeze_end, so gating here dropped the freeze-end snapshot and left only buytime_ended, by which point
		// the players who already died were missing. _current is reset every round_start and OnRoundEnd is gated, so
		// warmup/knife snapshots never reach the result.
		foreach (var controller in demo.Players)
		{
			if (DemoTeams.Side(controller.CSTeamNum) is not { } side || controller.PlayerPawn is not { IsAlive: true } pawn)
			{
				continue;
			}

			var steamId = (long)controller.SteamID;
			var snapshot = new DemoPlayerEconomy(
				steamId,
				controller.PlayerName,
				side,
				pawn.CurrentEquipmentValue,
				controller.InGameMoneyServices?.Account ?? 0,
				controller.InGameMoneyServices?.CashSpentThisRound ?? 0,
				pawn.ArmorValue,
				controller.PawnHasHelmet,
				controller.PawnHasDefuser,
				DemoWeapons.Primary(pawn));

			if (!_current.TryGetValue(steamId, out var previous) || snapshot.EquipmentValue >= previous.EquipmentValue)
			{
				_current[steamId] = snapshot;
			}
		}
	}

	private void OnRoundEnd()
	{
		if (!clock.IsOfficialMatchRound || _current.Count == 0)
		{
			return;
		}

		_rounds.RemoveAll(r => r.RoundNumber == clock.CurrentRoundNumber);
		_rounds.Add(new DemoRoundEconomy(clock.CurrentRoundNumber, _current.Values.ToList()));
	}

	#endregion
}
