using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.Calendar.Shared;
using HarnasHub.Application.Features.OpponentNotes.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.OpponentNotes.GetOpponentProfile;

/// <summary>Handles <see cref="GetOpponentProfileQuery"/>. An opponent with no data yet returns an empty profile rather than
/// NotFound, so a coach can open it straight from a freshly scheduled event and start adding notes.</summary>
public class GetOpponentProfileHandler(IApplicationDbContext dbContext)
	: IRequestHandler<GetOpponentProfileQuery, ErrorOr<OpponentProfileDto>>
{
	#region Public Methods

	public async Task<ErrorOr<OpponentProfileDto>> Handle(GetOpponentProfileQuery request, CancellationToken cancellationToken)
	{
		var key = OpponentNames.ToKey(request.Name);
		var now = DateTime.UtcNow;

		var notes = await dbContext.OpponentNotes
			.Where(n => n.OpponentName.Trim().ToLower() == key)
			.OrderByDescending(n => n.CreatedAtUtc)
			.Select(n => new OpponentNoteDto(n.Id, n.OpponentName, n.Content, n.MaterialUrl, n.CreatedAtUtc))
			.ToListAsync(cancellationToken);

		var matchRows = await dbContext.MatchResults
			.Where(m => m.Opponent.Trim().ToLower() == key)
			.OrderByDescending(m => m.PlayedAtUtc)
			.Select(m => new
			{
				m.Opponent,
				Dto = new OpponentMatchDto(
					m.Id, m.PlayedAtUtc, m.MapName, m.OurScore, m.OpponentScore, m.Category.ToString(), m.DemoUrl, m.Notes)
			})
			.ToListAsync(cancellationToken);

		var upcomingEvents = await dbContext.Events
			.Where(e => e.Opponent != null && e.Opponent.Trim().ToLower() == key && e.StartsAtUtc >= now)
			.OrderBy(e => e.StartsAtUtc)
			.Select(EventMappings.Projection)
			.ToListAsync(cancellationToken);

		var matches = matchRows.Select(r => r.Dto).ToList();
		var outcomes = matches.Select(m => OpponentNames.Outcome(m.OurScore, m.OpponentScore)).ToList();

		// The spelling the team used most recently, so the header matches what they typed last.
		var displayName = notes.Select(n => (Name: n.OpponentName, At: n.CreatedAtUtc))
			.Concat(matchRows.Select(r => (Name: r.Opponent, At: r.Dto.PlayedAtUtc)))
			.Concat(upcomingEvents.Select(e => (Name: e.Opponent!, At: e.StartsAtUtc)))
			.OrderByDescending(s => s.At)
			.Select(s => s.Name.Trim())
			.FirstOrDefault() ?? request.Name.Trim();

		return new OpponentProfileDto(
			displayName,
			outcomes.Sum(o => o.Wins),
			outcomes.Sum(o => o.Losses),
			outcomes.Sum(o => o.Draws),
			BuildMapRecords(matches),
			matches,
			notes,
			upcomingEvents);
	}

	#endregion

	#region Private Methods

	/// <summary>Per-map record, most-played map first; games with no map logged are left out.</summary>
	private static List<OpponentMapRecordDto> BuildMapRecords(List<OpponentMatchDto> matches) =>
		matches
			.Where(m => !string.IsNullOrWhiteSpace(m.MapName))
			.GroupBy(m => m.MapName!)
			.Select(group =>
			{
				var outcomes = group.Select(m => OpponentNames.Outcome(m.OurScore, m.OpponentScore)).ToList();
				return new OpponentMapRecordDto(
					group.Key,
					outcomes.Sum(o => o.Wins),
					outcomes.Sum(o => o.Losses),
					outcomes.Sum(o => o.Draws));
			})
			.OrderByDescending(r => r.Wins + r.Losses + r.Draws)
			.ThenBy(r => r.MapName)
			.ToList();

	#endregion
}
