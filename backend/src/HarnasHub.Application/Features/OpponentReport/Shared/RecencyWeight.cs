namespace HarnasHub.Application.Features.OpponentReport.Shared;

/// <summary>Exponential recency decay, so last season doesn't outweigh the current form: a game <see cref="HalfLifeDays"/> old counts
/// half, twice that old a quarter (the 120-day window ends at ~16%). Counts shown on screen stay raw; only rates/preferences are weighted.</summary>
public static class RecencyWeight
{
	#region Public Fields

	/// <summary>Half-life of a game's weight in days.</summary>
	public const double HalfLifeDays = 45;

	#endregion

	#region Public Methods

	/// <summary><c>0.5^(age / half-life)</c>, 1 for a future date or when <paramref name="nowUtc"/> is null (no weighting).</summary>
	public static double Of(DateTime playedAtUtc, DateTime? nowUtc)
	{
		if (nowUtc is not { } now)
		{
			return 1;
		}

		var ageDays = Math.Max(0, (now - playedAtUtc).TotalDays);
		return Math.Pow(0.5, ageDays / HalfLifeDays);
	}

	#endregion
}
