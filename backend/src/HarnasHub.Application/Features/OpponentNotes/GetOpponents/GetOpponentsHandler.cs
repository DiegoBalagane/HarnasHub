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

		var hidden = await dbContext.HiddenOpponents
			.Select(h => new { h.OpponentKey, h.DisplayName })
			.ToListAsync(cancellationToken);
		var hiddenKeys = hidden.Select(h => h.OpponentKey).ToHashSet();

		var summaries = mentions
			.GroupBy(m => OpponentNames.ToKey(m.Name))
			.Where(group => request.IncludeHidden || !hiddenKeys.Contains(group.Key))
			.Select(group => Summarize(group.ToList(), now) with { IsHidden = hiddenKeys.Contains(group.Key) })
			.ToList();

		// Opponents that exist only through scouting data (a FACEIT link or analysed demos) have no note/result/event yet.
		var listedKeys = summaries.Select(s => OpponentNames.ToKey(s.Name)).ToHashSet();
		var links = await dbContext.OpponentFaceitLinks
			.Select(l => new { l.OpponentKey, l.DisplayName })
			.ToListAsync(cancellationToken);
		var demoKeys = await dbContext.OpponentDemoAnalyses
			.Select(d => d.OpponentKey)
			.Distinct()
			.ToListAsync(cancellationToken);
		var linkNames = links.GroupBy(l => l.OpponentKey).ToDictionary(g => g.Key, g => g.First().DisplayName);

		foreach (var key in linkNames.Keys.Concat(demoKeys).Distinct())
		{
			if (listedKeys.Contains(key) || (!request.IncludeHidden && hiddenKeys.Contains(key)))
			{
				continue;
			}

			var name = linkNames.TryGetValue(key, out var linkName) && !string.IsNullOrWhiteSpace(linkName)
				? linkName.Trim()
				: hidden.FirstOrDefault(h => h.OpponentKey == key)?.DisplayName ?? key;
			summaries.Add(new OpponentSummaryDto(name, 0, 0, 0, 0, null, null, IsHidden: hiddenKeys.Contains(key)));
			listedKeys.Add(key);
		}

		if (request.IncludeHidden)
		{
			// A hidden opponent with no history left is still listed, so it can be restored.
			summaries.AddRange(hidden
				.Where(h => !listedKeys.Contains(h.OpponentKey))
				.Select(h => new OpponentSummaryDto(h.DisplayName, 0, 0, 0, 0, null, null, IsHidden: true)));
		}

		return summaries
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
