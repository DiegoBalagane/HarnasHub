using HarnasHub.Core.Enums;

namespace HarnasHub.Application.Features.OpponentReport.Shared;

/// <summary>Short Polish notes about each side of a map, used in the simulated veto step reasons.</summary>
public static class VetoNotes
{
	#region Public Methods

	/// <summary>Our side of a map in a few words.</summary>
	public static string Ours(MapPoolStatus? status, double ourWins, int ourTotal) =>
		status == MapPoolStatus.Ban
			? "u nas stały ban w puli map"
			: ourTotal == 0
				? "my: brak meczów"
				: $"my: {ourTotal} {MatchNoun(ourTotal)}, {Math.Round(ourWins / ourTotal * 100, 1):0}% wygranych";

	/// <summary>Their side of a map in a few words.</summary>
	public static string Theirs(MapMetrics their) =>
		their.Games == 0 ? "oni: nie grają" : $"oni: {their.Games} {MatchNoun(their.Games)}, {Math.Round((their.WinRate ?? 0) * 100, 1):0}% wygranych";

	/// <summary>Polish plural of "mecz": 1 mecz, 2–4 mecze (except 12–14), otherwise meczów.</summary>
	public static string MatchNoun(int count)
	{
		if (count == 1)
		{
			return "mecz";
		}

		var lastDigit = count % 10;
		var lastTwo = count % 100;
		return lastDigit is >= 2 and <= 4 && lastTwo is < 12 or > 14 ? "mecze" : "meczów";
	}

	#endregion
}
