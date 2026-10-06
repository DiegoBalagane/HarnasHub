using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Common.Faceit;
using HarnasHub.Application.Common.Demos;
using HarnasHub.Application.Features.MatchAnalysis.Shared;
using HarnasHub.Application.Features.Results.Shared;
using HarnasHub.Application.Features.Stats.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HarnasHub.Application.Features.Results.AnalyzeDemo;

/// <summary>Handles <see cref="AnalyzeDemoCommand"/> by parsing the demo and splitting its round-1 sides into two
/// candidate "our team" groups — whichever the roster's SteamID64s overlap with (if either) is suggested, but the
/// coach picks the final one client-side, so this never has to guess wrong the way pure SteamID matching can when
/// nobody's SteamID64 is on file yet.</summary>
public class AnalyzeDemoHandler(
	IDemoParser demoParser,
	IApplicationDbContext dbContext,
	IFileStorage fileStorage,
	FaceitDemoMatchLookup faceitLookup,
	ILogger<AnalyzeDemoHandler> logger)
	: IRequestHandler<AnalyzeDemoCommand, ErrorOr<AnalyzeDemoResultDto>>
{
	#region Public Methods

	public async Task<ErrorOr<AnalyzeDemoResultDto>> Handle(AnalyzeDemoCommand request, CancellationToken cancellationToken)
	{
		DemoTimeline timeline;

		try
		{
			// One pass with every collector: the score/stats preview needs only Stats, but the same read also yields the
			// full timeline, which is parked in storage so the result can keep it without uploading the demo again.
			timeline = await demoParser.ParseAsync(request.DemoStream, DemoParseOptions.MatchAnalysis, cancellationToken);
		}
		catch (Exception ex)
		{
			// Any parser failure (corrupt file, unsupported build, wrong file type) is a validation problem for the
			// caller, not a server error — the demo bytes themselves are untrusted input. Logged in full here since
			// the client only ever sees a generic "couldn't read this as a CS2 demo" message.
			logger.LogWarning(ex, "Nie udało się sparsować demki");
			return ResultErrors.InvalidDemoFile;
		}

		var parsed = timeline.Stats;
		if (parsed is null || parsed.RoundsPlayed == 0 || parsed.Rounds.Count == 0 || parsed.Players.Count == 0)
		{
			logger.LogWarning(
				"Demka sparsowana bez błędu, ale bez rozgrywki do zapisania: RoundsPlayed={RoundsPlayed}, Rounds={Rounds}, Players={Players}",
				parsed?.RoundsPlayed, parsed?.Rounds.Count, parsed?.Players.Count);
			return ResultErrors.InvalidDemoFile;
		}

		var firstRound = parsed.Rounds[0];
		var demoNames = parsed.Players.ToDictionary(p => p.SteamId64, p => (string?)p.PlayerName);
		foreach (var id in firstRound.TerroristSteamIds.Concat(firstRound.CounterTerroristSteamIds))
		{
			demoNames.TryAdd(id, null);
		}

		var nameBySteamId = await PlayerNameResolver.ResolveAsync(dbContext, demoNames, cancellationToken);

		string NameOrId(long steamId) => nameBySteamId.TryGetValue(steamId, out var name) ? name : PlayerNameResolver.Fallback(steamId);

		var teamA = BuildTeamPreview(parsed.Rounds, firstRound.TerroristSteamIds, NameOrId);
		var teamB = BuildTeamPreview(parsed.Rounds, firstRound.CounterTerroristSteamIds, NameOrId);

		var rosterSteamIds = await ResolveRosterSteamIdsAsync(cancellationToken);
		var suggestedTeam = firstRound.TerroristSteamIds.Any(rosterSteamIds.Contains)
			? "A"
			: firstRound.CounterTerroristSteamIds.Any(rosterSteamIds.Contains)
				? "B"
				: null;

		// A FACEIT file name pre-fills opponent/date/category; FACEIT knowing which faction is us also settles the team pick.
		var faceit = await faceitLookup.FindAsync(request.FileName, cancellationToken);
		var faceitPrefill = faceit.Match is { } faceitMatch
			? FaceitMatchPrefillBuilder.Build(faceitMatch, firstRound.TerroristSteamIds, firstRound.CounterTerroristSteamIds)
			: null;
		if (suggestedTeam is null && faceitPrefill?.OurFactionIndex is { } ourFaction)
		{
			suggestedTeam = faceitPrefill.Factions[ourFaction].DemoTeam;
		}

		var players = parsed.Players
			.Select(p => new AnalyzedDemoPlayerDto(
				p.SteamId64.ToString(),
				NameOrId(p.SteamId64),
				p.Kills,
				p.Deaths,
				p.Assists,
				p.Headshots,
				p.DamageDealt,
				p.EntryKills,
				p.EntryDeaths,
				p.KastRounds,
				p.UtilityDamage,
				p.FlashAssists,
				p.MultiKillRounds.GetValueOrDefault(2),
				p.MultiKillRounds.GetValueOrDefault(3),
				p.MultiKillRounds.GetValueOrDefault(4),
				p.MultiKillRounds.GetValueOrDefault(5),
				p.DeathPositions.Select(d => new DeathPositionDto(d.X, d.Y, d.Side.ToString())).ToList()))
			.ToList();

		var pendingTimelineKey = await ParkTimelineAsync(timeline, cancellationToken);
		return new AnalyzeDemoResultDto(
			parsed.RoundsPlayed, parsed.MapName?.ToString(), teamA, teamB, suggestedTeam, players, pendingTimelineKey,
			faceitPrefill, faceit.Note);
	}

	#endregion

	#region Private Methods

	/// <summary>Stores the timeline under a fresh pending key for <c>AddResultCommand</c> to claim; null when storage isn't
	/// configured or the upload failed — the preview itself must never fail just because the timeline couldn't be kept.</summary>
	private async Task<string?> ParkTimelineAsync(DemoTimeline timeline, CancellationToken cancellationToken)
	{
		if (!fileStorage.IsConfigured || timeline.Rounds.Count == 0)
		{
			return null;
		}

		var key = MatchTimelineStorage.NewPendingKey();
		try
		{
			await MatchTimelineStorage.SaveAsync(fileStorage, key, timeline, DemoTimelineSerializer.CurrentParserVersion, cancellationToken);
			return key;
		}
		catch (Exception ex) when (ex is not OperationCanceledException)
		{
			logger.LogWarning(ex, "Nie udało się zapisać tymczasowej osi czasu meczu");
			return null;
		}
	}

	/// <summary>A group's own would-be score, found by treating its round-1 members as "our roster" for the same
	/// per-round majority-side attribution <see cref="DemoScoreCalculator"/> uses for a real roster match — always
	/// resolves (never null) since a group's members are always present, by construction, in their own round 1.</summary>
	private static DemoTeamPreviewDto BuildTeamPreview(
		IReadOnlyList<DemoRoundResult> rounds,
		IReadOnlyList<long> roundOneSteamIds,
		Func<long, string> nameOrId)
	{
		var score = DemoScoreCalculator.Calculate(rounds, roundOneSteamIds.ToHashSet()) ?? (0, 0);
		return new DemoTeamPreviewDto(
			roundOneSteamIds.Select(nameOrId).ToList(),
			roundOneSteamIds.Select(id => id.ToString()).ToList(),
			score.OurScore,
			score.OpponentScore);
	}

	/// <summary>Loads every roster member who has told us their SteamID64 — used only to suggest which team split is
	/// probably "ours"; the coach's own pick in the UI is what actually decides it.</summary>
	private async Task<HashSet<long>> ResolveRosterSteamIdsAsync(CancellationToken cancellationToken)
	{
		var steamIds = await dbContext.Users
			.Where(u => u.SteamId64 != null)
			.Select(u => u.SteamId64!)
			.ToListAsync(cancellationToken);

		return steamIds
			.Select(id => long.TryParse(id, out var parsed) ? parsed : (long?)null)
			.Where(id => id.HasValue)
			.Select(id => id!.Value)
			.ToHashSet();
	}

	#endregion
}
