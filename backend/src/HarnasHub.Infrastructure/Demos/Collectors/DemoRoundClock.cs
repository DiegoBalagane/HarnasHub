#region Usings

using DemoFile;

#endregion

namespace HarnasHub.Infrastructure.Demos.Collectors;

/// <summary>Round bookkeeping every collector shares: whether the round in progress counts towards the match, its
/// 1-based number, and when it started / left freeze time. Must be subscribed BEFORE any collector, so that by the time
/// a collector's own round_start handler runs the latch already reflects the new round.</summary>
internal sealed class DemoRoundClock(CsDemoParser demo)
{
	#region Private Fields

	// Whether the round in progress counts towards the match, latched at round_start; null until this recording
	// contains one, in which case the live game-rules state is consulted instead.
	private bool? _latchedRoundIsOfficial;
	private int _completedOfficialRounds;

	#endregion

	#region Public Properties

	/// <summary>1-based number of the round in progress (or the one that just ended, until the next round_start).</summary>
	public int CurrentRoundNumber { get; private set; } = 1;

	/// <summary>Game time the current round started at; null when the recording began mid-round.</summary>
	public float? RoundStartTime { get; private set; }

	/// <summary>Game time freeze time ended at in the current round; null until it does (or if it wasn't recorded).</summary>
	public float? FreezeEndTime { get; private set; }

	/// <summary>Current game time in seconds.</summary>
	public float Now => demo.CurrentGameTime.Value;

	/// <summary>Warmup and the knife round must never reach the totals — they inflate kills/damage (warmup respawns
	/// endlessly) and the round count that ADR/KAST/Rating are divided by. Read live off CCSGameRules rather than
	/// latched onto warmup_end, for the same reason team sides are read live at round_end: a demo can start recording
	/// after those one-shot transitions already happened and would then never observe them.</summary>
	public bool IsOfficialMatchRound =>
		_latchedRoundIsOfficial ?? demo.GameRules is { WarmupPeriod: false, HasMatchStarted: true };

	#endregion

	#region Public Methods

	/// <summary>Hooks the clock onto the parser; call before any collector subscribes.</summary>
	public void Subscribe()
	{
		demo.Source1GameEvents.RoundAnnounceMatchStart += _ => OnMatchStart();
		demo.Source1GameEvents.RoundStart += _ => OnRoundStart();
		demo.Source1GameEvents.RoundFreezeEnd += _ => FreezeEndTime = Now;
		demo.Source1GameEvents.RoundEnd += _ => OnRoundEnd();
	}

	/// <summary>Seconds since freeze end (falling back to round start, then to zero), never negative.</summary>
	public float SecondsIntoRound()
	{
		var origin = FreezeEndTime ?? RoundStartTime ?? Now;
		return Math.Max(0f, Now - origin);
	}

	#endregion

	#region Private Methods

	private void OnMatchStart()
	{
		// The round already in progress *is* match round 1: CCSGameRules only flips HasMatchStarted a moment later,
		// so it gets latched as official explicitly.
		_completedOfficialRounds = 0;
		CurrentRoundNumber = 1;
		_latchedRoundIsOfficial = true;
	}

	private void OnRoundStart()
	{
		CurrentRoundNumber = _completedOfficialRounds + 1;
		RoundStartTime = Now;
		FreezeEndTime = null;

		// Latched here and reused for the whole round: CCSGameRules clears HasMatchStarted the moment a match is
		// decided, which happens *before* the final round's round_end fires — judging that event live would silently
		// drop the very round that won the match from both the score and everyone's totals.
		_latchedRoundIsOfficial = demo.GameRules is { WarmupPeriod: false, HasMatchStarted: true };
	}

	private void OnRoundEnd()
	{
		// CurrentRoundNumber deliberately stays put until the next round_start, so collectors handling this same
		// round_end (and anything thrown in the post-round seconds) still see the round that just ended.
		if (IsOfficialMatchRound)
		{
			_completedOfficialRounds++;
		}
	}

	#endregion
}
