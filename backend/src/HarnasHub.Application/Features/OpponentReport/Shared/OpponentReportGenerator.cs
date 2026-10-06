using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.OpponentNotes.Shared;
using HarnasHub.Application.Features.OpponentReport.Tendencies;
using HarnasHub.Application.Features.Veto.Shared;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.OpponentReport.Shared;

/// <summary>Loads everything the report needs from the database and hands it to <see cref="OpponentReportBuilder"/>.</summary>
public static class OpponentReportGenerator
{
	#region Public Methods

	/// <summary>Builds a fresh report for the opponent from the cache — no FACEIT calls; live fields are not filled.</summary>
	public static async Task<OpponentReportDto> BuildAsync(
		IApplicationDbContext dbContext,
		string opponentName,
		DateTime nowUtc,
		CancellationToken cancellationToken)
	{
		var key = OpponentNames.ToKey(opponentName);
		var link = await dbContext.OpponentFaceitLinks.AsNoTracking().FirstOrDefaultAsync(l => l.OpponentKey == key, cancellationToken);
		var theirIds = link?.PlayerIds ?? [];

		var theirPlayers = await dbContext.FaceitPlayers.AsNoTracking()
			.Where(p => theirIds.Contains(p.Id))
			.ToListAsync(cancellationToken);
		var linkDto = link is null
			? null
			: new OpponentFaceitLinkDto(
				link.DisplayName,
				link.FaceitTeamId,
				theirIds
					.Select(id => theirPlayers.FirstOrDefault(p => p.Id == id))
					.Where(p => p is not null)
					.Select(p => new FaceitPlayerDto(p!.Id, p.Nickname, p.Elo, p.SkillLevel))
					.ToList(),
				link.LinkedAtUtc,
				link.LastSyncedAtUtc);

		var ourPlayers = (await OurRosterFaceit.LoadCachedAsync(dbContext, cancellationToken)).Players
			.Select(p => new FaceitPlayerDto(p.Id, p.Nickname, p.Elo, p.SkillLevel))
			.ToList();
		var ourIds = ourPlayers.Select(p => p.PlayerId).Distinct().ToList();

		var since = nowUtc - FaceitSync.HistoryWindow;
		var matches = await dbContext.FaceitMatches.AsNoTracking()
			.Where(m => m.PlayedAtUtc >= since)
			.ToListAsync(cancellationToken);
		var theirStats = await dbContext.FaceitMatchPlayerStats.AsNoTracking()
			.Where(s => theirIds.Contains(s.PlayerId))
			.ToListAsync(cancellationToken);
		// Our players' lines for their individual form, limited to the window by joining the cached maps.
		var ourStats = await dbContext.FaceitMatchPlayerStats.AsNoTracking()
			.Where(s => ourIds.Contains(s.PlayerId))
			.Join(dbContext.FaceitMatches.Where(m => m.PlayedAtUtc >= since), s => s.MatchId, m => m.Id, (s, _) => s)
			.ToListAsync(cancellationToken);

		var vetoData = await VetoInputLoader.LoadAsync(dbContext, key, cancellationToken);

		return OpponentReportBuilder.Build(new OpponentReportInput(
			link?.DisplayName ?? opponentName.Trim(),
			linkDto,
			nowUtc,
			matches,
			theirStats,
			theirIds.ToHashSet(),
			ourIds.ToHashSet(),
			vetoData.Inputs,
			ourStats,
			ourPlayers,
			await FaceitLifetimeStats.LoadAsync(dbContext, theirIds, cancellationToken),
			await FaceitLifetimeStats.LoadAsync(dbContext, ourIds, cancellationToken)));
	}

	/// <summary>The opponent's active lineup ids (<see cref="ActiveLineupResolver"/>) from the cached team games of the linked
	/// <paramref name="playerIds"/> — used by the sync to fetch lifetime stats only for players who actually play.</summary>
	public static async Task<IReadOnlySet<string>> ActiveLineupIdsAsync(
		IApplicationDbContext dbContext,
		IReadOnlyCollection<string> playerIds,
		DateTime nowUtc,
		CancellationToken cancellationToken)
	{
		var since = nowUtc - FaceitSync.HistoryWindow;
		var matches = await dbContext.FaceitMatches.AsNoTracking()
			.Where(m => m.PlayedAtUtc >= since)
			.ToListAsync(cancellationToken);
		var roster = playerIds.ToHashSet();
		var games = TeamMatchDetector.Detect(matches, roster);
		return ActiveLineupResolver.Resolve(games, roster, nowUtc, new Dictionary<string, string>(), []).ActiveIds;
	}

	/// <summary>Fills the fields that must reflect the present rather than the snapshot: whether FACEIT is configured and the next
	/// scheduled event against the opponent, plus the demo tendencies.</summary>
	public static async Task<OpponentReportDto> WithLiveFieldsAsync(
		IApplicationDbContext dbContext,
		OpponentReportDto report,
		bool faceitConfigured,
		DateTime nowUtc,
		CancellationToken cancellationToken)
	{
		var key = OpponentNames.ToKey(report.OpponentName);
		var nextEvent = await dbContext.Events.AsNoTracking()
			.Where(e => e.Opponent != null && e.Opponent.Trim().ToLower() == key && e.StartsAtUtc >= nowUtc)
			.OrderBy(e => e.StartsAtUtc)
			.Select(e => new { e.Id, e.StartsAtUtc })
			.FirstOrDefaultAsync(cancellationToken);

		var tendencies = await OpponentTendencyLoader.LoadAsync(dbContext, key, cancellationToken);
		var unresolved = (await OurRosterFaceit.LoadCachedAsync(dbContext, cancellationToken)).Unresolved;

		return report with
		{
			FaceitConfigured = faceitConfigured,
			NextEventId = nextEvent?.Id,
			NextEventAtUtc = nextEvent?.StartsAtUtc,
			Tendencies = tendencies,
			UnresolvedOurPlayers = unresolved
		};
	}

	#endregion
}
