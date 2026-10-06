#region Usings

using DemoFile;
using DemoFile.Game.Cs;
using HarnasHub.Application.Abstractions;

#endregion

namespace HarnasHub.Infrastructure.Demos.Collectors;

/// <summary>Every death in an official round with time, killer/victim/assister, both positions, weapon and the event's
/// own modifiers (headshot, wallbang, through smoke, no-scope, attacker blind). Marks the round's first enemy kill as the
/// opening; trades and clutches are derived later from this list in pure code.</summary>
internal sealed class KillCollector(CsDemoParser demo, DemoRoundClock clock) : IDemoCollector
{
	#region Private Types

	private sealed record RawParticipant(long SteamId64, string Name, Core.Enums.MapSide? Side, (float X, float Y, float Z)? World);

	private sealed record RawKill(DemoKill Kill, RawParticipant? Killer, RawParticipant Victim);

	#endregion

	#region Private Fields

	private readonly List<RawKill> _kills = [];

	// Same reasoning as StatsCollector: the dying pawn's own update for the death tick already reports it dead, so the
	// last position seen while alive is where the player actually died.
	private readonly Dictionary<ulong, (float X, float Y, float Z)> _lastKnownPosition = [];
	private bool _openingTaken;

	#endregion

	#region Public Methods

	/// <inheritdoc />
	public void Subscribe()
	{
		demo.Source1GameEvents.RoundAnnounceMatchStart += _ => _kills.Clear();
		demo.Source1GameEvents.RoundStart += _ =>
		{
			_openingTaken = false;
			_lastKnownPosition.Clear();
		};
		demo.Source1GameEvents.PlayerDeath += OnPlayerDeath;
		demo.EntityEvents.CCSPlayerPawn.PostUpdate += OnPlayerPawnUpdate;
	}

	/// <inheritdoc />
	public void Contribute(DemoTimelineBuilder builder)
	{
		foreach (var raw in _kills)
		{
			builder.Kills.Add(raw.Kill with
			{
				Killer = raw.Killer is null ? null : ToPublic(raw.Killer, builder),
				Victim = ToPublic(raw.Victim, builder)
			});
		}
	}

	#endregion

	#region Private Methods

	private void OnPlayerPawnUpdate(CCSPlayerPawn pawn)
	{
		if (pawn is { IsAlive: true, Health: > 0, OriginalController: { } controller })
		{
			_lastKnownPosition[controller.SteamID] = (pawn.Origin.X, pawn.Origin.Y, pawn.Origin.Z);
		}
	}

	private void OnPlayerDeath(Source1PlayerDeathEvent e)
	{
		if (!clock.IsOfficialMatchRound || e.Player is not { } victim)
		{
			return;
		}

		var attacker = e.Attacker is { } a && !ReferenceEquals(a, victim) ? a : null;
		var isTeamKill = attacker is not null && attacker.CSTeamNum == victim.CSTeamNum;
		var isOpening = attacker is not null && !isTeamKill && !_openingTaken;
		_openingTaken |= isOpening;

		var victimRaw = Participant(victim, null);
		var killerRaw = attacker is null ? null : Participant(attacker, e.AttackerPawn);
		var assister = e.Assister is { } assist ? new DemoKillParticipant((long)assist.SteamID, assist.PlayerName, DemoTeams.Side(assist.CSTeamNum), null) : null;

		var kill = new DemoKill(
			clock.CurrentRoundNumber,
			clock.SecondsIntoRound(),
			null,
			new DemoKillParticipant(victimRaw.SteamId64, victimRaw.Name, victimRaw.Side, null),
			assister,
			string.IsNullOrWhiteSpace(e.Weapon) ? "world" : e.Weapon,
			e.Headshot,
			e.Penetrated > 0,
			e.Thrusmoke,
			e.Noscope,
			e.Attackerblind,
			e.Assistedflash,
			isOpening,
			isTeamKill);

		_kills.Add(new RawKill(kill, killerRaw, victimRaw));
	}

	private RawParticipant Participant(CCSPlayerController controller, CCSPlayerPawn? livePawn)
	{
		(float, float, float)? world = livePawn is { IsAlive: true }
			? (livePawn.Origin.X, livePawn.Origin.Y, livePawn.Origin.Z)
			: _lastKnownPosition.TryGetValue(controller.SteamID, out var last) ? last : null;

		return new RawParticipant((long)controller.SteamID, controller.PlayerName, DemoTeams.Side(controller.CSTeamNum), world);
	}

	private static DemoKillParticipant ToPublic(RawParticipant raw, DemoTimelineBuilder builder) => new(
		raw.SteamId64,
		raw.Name,
		raw.Side,
		raw.World is { } w ? builder.ToPosition(w.X, w.Y, w.Z) : null);

	#endregion
}
