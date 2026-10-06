#region Usings

using DemoFile;
using DemoFile.Game.Cs;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Common.Demos;

#endregion

namespace HarnasHub.Infrastructure.Demos.Collectors;

/// <summary>Samples every alive player's position, side, health and active weapon once per whole second of round time
/// (from freeze end until round end) in official rounds — the raw material for opponent tendencies (CT setups, T
/// entries) and the stage-6 2D replay. Checked at the end of every demo command (≈ every tick), sampling the first command
/// at or after each whole second; track bookkeeping lives in the pure <see cref="PositionTrackAccumulator"/>.</summary>
internal sealed class PositionSampler(CsDemoParser demo, DemoRoundClock clock) : IDemoCollector
{
	#region Private Fields

	private readonly PositionTrackAccumulator _tracks = new();
	private bool _sampling;
	private int _nextSecond;

	#endregion

	#region Public Methods

	/// <inheritdoc />
	public void Subscribe()
	{
		demo.Source1GameEvents.RoundAnnounceMatchStart += _ =>
		{
			_tracks.Reset();
			_sampling = false;
		};
		demo.Source1GameEvents.RoundStart += _ => StopRound();
		demo.Source1GameEvents.RoundFreezeEnd += _ =>
		{
			StopRound();
			_sampling = clock.IsOfficialMatchRound;
			_nextSecond = 0;
		};
		demo.Source1GameEvents.RoundEnd += _ => StopRound();
		demo.OnCommandFinishPersistent += OnCommandFinish;
	}

	/// <inheritdoc />
	public void Contribute(DemoTimelineBuilder builder) =>
		builder.Positions.AddRange(_tracks.Build((x, y) => builder.MapName is { } map ? MapCalibration.ToRadarFraction(map, x, y) : null));

	#endregion

	#region Private Methods

	private void StopRound()
	{
		_sampling = false;
		_tracks.EndRound();
	}

	private void OnCommandFinish()
	{
		if (!_sampling || clock.FreezeEndTime is null)
		{
			return;
		}

		var second = (int)MathF.Floor(clock.SecondsIntoRound());
		if (second < _nextSecond)
		{
			return;
		}

		_nextSecond = second + 1;
		var round = clock.CurrentRoundNumber;

		foreach (var controller in demo.Players)
		{
			// Bots share SteamID 0 and would merge into one track, so only real accounts are sampled.
			if (controller.SteamID == 0
				|| DemoTeams.Side(controller.CSTeamNum) is not { } side
				|| controller.PlayerPawn is not { IsAlive: true, Health: > 0 } pawn)
			{
				continue;
			}

			var weapon = pawn.ActiveWeapon is CCSWeaponBase active ? DemoWeapons.Name(active) : null;
			_tracks.Add(round, second, (long)controller.SteamID, side, pawn.Origin.X, pawn.Origin.Y, pawn.Origin.Z, pawn.Health, weapon);
		}

		_tracks.EndSecond(second);
	}

	#endregion
}
