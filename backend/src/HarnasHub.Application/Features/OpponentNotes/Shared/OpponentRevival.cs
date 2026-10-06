#region Usings

using HarnasHub.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

#endregion

namespace HarnasHub.Application.Features.OpponentNotes.Shared;

/// <summary>Brings a deleted opponent back onto the list the moment it's used again — a new note, FACEIT link, event or
/// result under that name — so re-adding a team after deleting it simply starts fresh (its scouting data was removed on
/// delete) instead of staying invisible. Only stages the change; the caller's SaveChanges persists it.</summary>
public static class OpponentRevival
{
	#region Public Methods

	/// <summary>Removes the hidden marker for <paramref name="opponentName"/>, if any.</summary>
	public static async Task ReviveAsync(IApplicationDbContext dbContext, string? opponentName, CancellationToken cancellationToken)
	{
		if (string.IsNullOrWhiteSpace(opponentName))
		{
			return;
		}

		var key = OpponentNames.ToKey(opponentName);
		var hidden = await dbContext.HiddenOpponents.FirstOrDefaultAsync(h => h.OpponentKey == key, cancellationToken);
		if (hidden is not null)
		{
			dbContext.HiddenOpponents.Remove(hidden);
		}
	}

	/// <summary>Like <see cref="ReviveAsync"/>, but only when an edit actually changed the opponent's name — editing an
	/// old match of a deleted opponent must not bring it back.</summary>
	public static Task ReviveIfRenamedAsync(
		IApplicationDbContext dbContext, string? previousName, string? newName, CancellationToken cancellationToken) =>
		OpponentNames.ToKey(previousName ?? string.Empty) == OpponentNames.ToKey(newName ?? string.Empty)
			? Task.CompletedTask
			: ReviveAsync(dbContext, newName, cancellationToken);

	#endregion
}
