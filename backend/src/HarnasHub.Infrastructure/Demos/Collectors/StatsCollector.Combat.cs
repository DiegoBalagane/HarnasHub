#region Usings

using DemoFile.Game.Cs;
using HarnasHub.Core.Enums;

#endregion

namespace HarnasHub.Infrastructure.Demos.Collectors;

/// <summary>Combat handlers of <see cref="StatsCollector"/>: positions, deaths, spawns and damage.</summary>
internal sealed partial class StatsCollector
{
	#region Private Methods

	private void OnPlayerPawnUpdate(CCSPlayerPawn pawn)
	{
		if (pawn is { IsAlive: true, Health: > 0, OriginalController: { } controller })
		{
			_lastKnownPosition[controller.SteamID] = (pawn.Origin.X, pawn.Origin.Y);
		}
	}

	private void OnPlayerDeath(Source1PlayerDeathEvent e)
	{
		if (!clock.IsOfficialMatchRound)
		{
			return;
		}

		if (e.Player is { } victim)
		{
			var victimAcc = GetOrAddAccumulator(victim);
			victimAcc.Deaths++;
			_roundDied.Add(victim.SteamID);

			if (!_roundHasEntryEvent)
			{
				victimAcc.EntryDeaths++;
			}

			var side = victim.CSTeamNum == CSTeamNumber.Terrorist ? MapSide.T : MapSide.CT;
			if (_lastKnownPosition.TryGetValue(victim.SteamID, out var position))
			{
				victimAcc.RawDeathPositions.Add((position.X, position.Y, side));
			}
		}

		// Suicides/world kills (Attacker null or the victim itself) don't credit anyone, and don't count as an entry.
		if (e.Attacker is { } attacker && !ReferenceEquals(attacker, e.Player))
		{
			var attackerAcc = GetOrAddAccumulator(attacker);
			attackerAcc.Kills++;
			_roundKilledOrAssisted.Add(attacker.SteamID);

			if (e.Headshot)
			{
				attackerAcc.Headshots++;
			}

			_roundKills.TryGetValue(attacker.SteamID, out var killsSoFar);
			_roundKills[attacker.SteamID] = killsSoFar + 1;

			if (!_roundHasEntryEvent)
			{
				_roundHasEntryEvent = true;
				attackerAcc.EntryKills++;
			}
		}

		if (e.Assister is { } assister)
		{
			var assisterAcc = GetOrAddAccumulator(assister);
			assisterAcc.Assists++;
			_roundKilledOrAssisted.Add(assister.SteamID);

			if (e.Assistedflash)
			{
				assisterAcc.FlashAssists++;
			}
		}
	}

	private void OnPlayerSpawn(Source1PlayerSpawnEvent e)
	{
		if (e.Player is { } player)
		{
			_healthBeforeNextHit[player.SteamID] = FullHealth;
		}
	}

	private void OnPlayerHurt(Source1PlayerHurtEvent e)
	{
		if (e.Player is not { } victim)
		{
			return;
		}

		// player_hurt reports the weapon's *raw* damage, not the health the victim actually lost: an AWP chest shot
		// on a full-health player reports 115 and a Deagle headshot well over 200. Tracking what the victim had left
		// (this event's own Health field is their health after the hit, so it seeds the next one) and capping against
		// it is what keeps ADR comparable to HLTV's — uncapped it ran roughly 20% high on real demos. Tracked for
		// every hit, including world/fall damage that credits nobody, or the running total would drift.
		var healthBefore = _healthBeforeNextHit.TryGetValue(victim.SteamID, out var tracked) ? tracked : FullHealth;
		_healthBeforeNextHit[victim.SteamID] = e.Health;

		if (!clock.IsOfficialMatchRound || e.Attacker is not { } attacker || ReferenceEquals(attacker, victim))
		{
			return;
		}

		// Friendly fire isn't a contribution to the round, so it stays out of ADR the same way HLTV keeps it out.
		if (attacker.CSTeamNum == victim.CSTeamNum)
		{
			return;
		}

		var damage = Math.Clamp(e.DmgHealth, 0, healthBefore);
		var accumulator = GetOrAddAccumulator(attacker);
		accumulator.DamageDealt += damage;

		if (e.Weapon is "hegrenade" or "inferno" or "molotov")
		{
			accumulator.UtilityDamage += damage;
		}
	}

	#endregion
}
