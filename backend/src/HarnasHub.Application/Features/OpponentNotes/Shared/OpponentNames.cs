namespace HarnasHub.Application.Features.OpponentNotes.Shared;

/// <summary>Opponent names are free text typed in three places (notes, results, events); this is the one rule for deciding two spellings mean the same team.</summary>
public static class OpponentNames
{
	#region Public Methods

	/// <summary>Case- and whitespace-insensitive grouping key — "Team X " and "team x" are the same opponent. Matches the
	/// <c>name.Trim().ToLower()</c> comparison used inside EF queries.</summary>
	public static string ToKey(string name) => name.Trim().ToLowerInvariant();

	/// <summary>Win/loss/draw from a single score line.</summary>
	public static (int Wins, int Losses, int Draws) Outcome(int ourScore, int opponentScore) =>
		ourScore > opponentScore ? (1, 0, 0) : ourScore < opponentScore ? (0, 1, 0) : (0, 0, 1);

	#endregion
}
