using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.OpponentNotes.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.OpponentNotes.GetOpponents;

/// <summary>Handles <see cref="GetOpponentsQuery"/> by merging opponent names from notes, results and events, grouped by <see cref="OpponentNames.ToKey"/>.</summary>
public class GetOpponentsHandler(IApplicationDbContext dbContext)
	: IRequestHandler<GetOpponentsQuery, ErrorOr<List<OpponentSummaryDto>>>
{
	#region Public Methods

	public async Task<ErrorOr<List<OpponentSummaryDto>>> Handle(GetOpponentsQuery request, CancellationToken cancellationToken)
	{
		var now = DateTime.UtcNow;

		var notes = await dbContext.OpponentNotes
			.Select(n => new { n.OpponentName, n.CreatedAtUtc })
			.ToListAsync(cancellationToken);

		var matches = await dbContext.MatchResults
			.Select(m => new { m.Opponent, m.PlayedAtUtc, m.OurScore, m.OpponentScore })
			.ToListAsync(cancellationToken);

		var events = await dbContext.Events
			.Where(e => e.Opponent != null)
			.Select(e => new { Opponent = e.Opponent!, e.StartsAtUtc })
			.ToListAsync(cancellationToken);

		var mentions = notes.Select(n => new OpponentMention(n.OpponentName, n.CreatedAtUtc, null, null, false, IsNote: true))
			.Concat(matches.Select(m => new OpponentMention(m.Opponent, m.PlayedAtUtc, m.OurScore, m.OpponentScore, false, IsNote: false)))
			.Concat(events.Select(e => new OpponentMention(e.Opponent, e.StartsAtUtc, null, null, IsEvent: true, IsNote: false)))
			.Where(m => !string.IsNullOrWhiteSpace(m.Name));

		return mentions
			.GroupBy(m => OpponentNames.ToKey(m.Name))
			.Select(group => Summarize(group.ToList(), now))
			// Upcoming games first (soonest on top), then everyone else by most recent game played.
			.OrderBy(s => s.NextEventAtUtc ?? DateTime.MaxValue)
			.ThenByDescending(s => s.LastPlayedAtUtc ?? DateTime.MinValue)
			.ThenBy(s => s.Name)
			.ToList();
	}

	#endregion

	#region Private Methods

	/// <summary>Folds every mention of one opponent into its summary row; the most recently used spelling becomes the display name.</summary>
	private static OpponentSummaryDto Summarize(List<OpponentMention> mentions, DateTime now)
	{
		var played = mentions.Where(m => m.OurScore.HasValue).ToList();
		var outcomes = played.Select(m => OpponentNames.Outcome(m.OurScore!.Value, m.OpponentScore!.Value)).ToList();

		return new OpponentSummaryDto(
			mentions.MaxBy(m => m.At)!.Name.Trim(),
			mentions.Count(m => m.IsNote),
			outcomes.Sum(o => o.Wins),
			outcomes.Sum(o => o.Losses),
			outcomes.Sum(o => o.Draws),
			played.Count == 0 ? null : played.Max(m => m.At),
			mentions.Where(m => m.IsEvent && m.At >= now).Select(m => (DateTime?)m.At).Min());
	}

	#endregion

	#region Nested Types

	/// <summary>One place an opponent name was typed — a note, a logged result (with its score) or a scheduled event.</summary>
	private sealed record OpponentMention(string Name, DateTime At, int? OurScore, int? OpponentScore, bool IsEvent, bool IsNote);

	#endregion
}
