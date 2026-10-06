#region Usings

using DemoFile;
using DemoFile.Game.Cs;
using HarnasHub.Application.Abstractions;

#endregion

namespace HarnasHub.Infrastructure.Demos.Collectors;

/// <summary>Every flash blinding in an official round (the <c>player_blind</c> event): who threw, who was blinded, for how
/// long, and whether it hit a teammate. Aggregation (enemies flashed, average duration) happens later in pure code.</summary>
internal sealed class BlindCollector(CsDemoParser demo, DemoRoundClock clock) : IDemoCollector
{
	#region Private Fields

	private readonly List<DemoBlind> _blinds = [];

	#endregion

	#region Public Methods

	/// <inheritdoc />
	public void Subscribe()
	{
		demo.Source1GameEvents.RoundAnnounceMatchStart += _ => _blinds.Clear();
		demo.Source1GameEvents.PlayerBlind += OnPlayerBlind;
	}

	/// <inheritdoc />
	public void Contribute(DemoTimelineBuilder builder) => builder.Blinds.AddRange(_blinds);

	#endregion

	#region Private Methods

	private void OnPlayerBlind(Source1PlayerBlindEvent e)
	{
		if (!clock.IsOfficialMatchRound || e.Player is not { } victim || e.Attacker is not { } attacker || e.BlindDuration <= 0f)
		{
			return;
		}

		var attackerSide = DemoTeams.Side(attacker.CSTeamNum);
		var victimSide = DemoTeams.Side(victim.CSTeamNum);

		_blinds.Add(new DemoBlind(
			clock.CurrentRoundNumber,
			clock.SecondsIntoRound(),
			new DemoKillParticipant((long)attacker.SteamID, attacker.PlayerName, attackerSide, null),
			new DemoKillParticipant((long)victim.SteamID, victim.PlayerName, victimSide, null),
			e.BlindDuration,
			attackerSide is not null && attackerSide == victimSide));
	}

	#endregion
}
