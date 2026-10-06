#region Usings

using System.Text;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.MatchAnalysis.Shared;
using HarnasHub.Application.Features.OpponentReport.Tendencies;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using Microsoft.EntityFrameworkCore;

#endregion

namespace HarnasHub.Application.Features.OpponentReport.Shared;

/// <summary>Where an opponent demo came from and whom it belongs to.</summary>
public record OpponentDemoInput(
	string OpponentKey,
	OpponentDemoSource Source,
	string? FaceitMatchId,
	int? FaceitMapNumber,
	DateTime? PlayedAtUtc,
	Guid? UserId)
{
	/// <summary>SteamID64s known to be the opponent in this very demo (e.g. its FACEIT room), tried before any other detection.</summary>
	public IReadOnlySet<long>? OpponentSteamIdsHint { get; init; }
}

/// <summary>Shared by the upload and FACEIT download paths: stores a parsed opponent timeline in object storage, detects
/// which team is the opponent, extracts the tendency facts and writes the <see cref="OpponentDemoAnalysis"/> row.</summary>
public static class OpponentDemoProcessor
{
	#region Public Methods

	/// <summary>Object key of an opponent timeline: <c>opponents/{slug of key}/{id}.json.gz</c>.</summary>
	public static string TimelineKey(string opponentKey, Guid id) => $"opponents/{Slug(opponentKey)}/{id:N}.json.gz";

	/// <summary>Opponent key reduced to [a-z0-9-] so it is a safe storage path segment (ids keep keys unique anyway).</summary>
	public static string Slug(string opponentKey)
	{
		var builder = new StringBuilder();
		foreach (var c in opponentKey.ToLowerInvariant())
		{
			var keep = c is >= 'a' and <= 'z' or >= '0' and <= '9';
			if (keep || (builder.Length > 0 && builder[^1] != '-'))
			{
				builder.Append(keep ? c : '-');
			}
		}

		var slug = builder.ToString().Trim('-');
		slug = slug.Length > 60 ? slug[..60].TrimEnd('-') : slug;
		return slug.Length == 0 ? "opponent" : slug;
	}

	/// <summary>SteamID64s known to be the opponent: their linked FACEIT players plus every earlier confirmed demo.</summary>
	public static async Task<HashSet<long>> KnownOpponentIdsAsync(IApplicationDbContext dbContext, string opponentKey, CancellationToken cancellationToken)
	{
		var link = await dbContext.OpponentFaceitLinks.AsNoTracking().FirstOrDefaultAsync(l => l.OpponentKey == opponentKey, cancellationToken);
		var playerIds = link?.PlayerIds ?? [];

		var steamIds = await dbContext.FaceitPlayers.AsNoTracking()
			.Where(p => playerIds.Contains(p.Id) && p.SteamId64 != null)
			.Select(p => p.SteamId64!)
			.ToListAsync(cancellationToken);
		var earlier = await dbContext.OpponentDemoAnalyses.AsNoTracking()
			.Where(a => a.OpponentKey == opponentKey)
			.Select(a => a.OpponentSteamIds)
			.ToListAsync(cancellationToken);

		var known = steamIds.Select(s => long.TryParse(s.Trim(), out var id) ? id : 0).Where(id => id != 0).ToHashSet();
		known.UnionWith(earlier.SelectMany(ids => ids));
		return known;
	}

	/// <summary>Uploads the timeline, detects the opponent's team and adds the analysis row (the caller saves changes).</summary>
	public static async Task<OpponentDemoAnalysis> StoreAsync(
		IApplicationDbContext dbContext,
		IFileStorage fileStorage,
		OpponentDemoInput input,
		DemoTimeline timeline,
		DateTime nowUtc,
		CancellationToken cancellationToken)
	{
		var id = Guid.NewGuid();
		var names = OpponentPlayerFactsExtractor.Names(timeline);
		var teamA = OpponentSideDetector.TeamRoster(timeline.Rounds, OpponentSideDetector.TeamA).ToList();
		var teamB = OpponentSideDetector.TeamRoster(timeline.Rounds, OpponentSideDetector.TeamB).ToList();

		var analysis = new OpponentDemoAnalysis
		{
			Id = id,
			OpponentKey = input.OpponentKey,
			MapName = timeline.MapName,
			RawMapName = timeline.RawMapName,
			PlayedAtUtc = input.PlayedAtUtc,
			Source = input.Source,
			FaceitMatchId = input.FaceitMatchId,
			FaceitMapNumber = input.FaceitMapNumber,
			TimelineObjectKey = TimelineKey(input.OpponentKey, id),
			ParserVersion = DemoTimelineSerializer.CurrentParserVersion,
			RoundsCount = timeline.Rounds.Count,
			TeamASteamIds = teamA,
			TeamBSteamIds = teamB,
			TeamANames = teamA.Select(s => names.GetValueOrDefault(s) ?? s.ToString()).ToList(),
			TeamBNames = teamB.Select(s => names.GetValueOrDefault(s) ?? s.ToString()).ToList(),
			CreatedByUserId = input.UserId,
			CreatedAtUtc = nowUtc
		};

		// The FACEIT room of the demo (when recognised) names the opponent's players outright — stronger than any history.
		var team = input.OpponentSteamIdsHint is { Count: > 0 } hint ? OpponentSideDetector.Detect(timeline.Rounds, hint) : null;
		team ??= await DetectTeamAsync(dbContext, input.OpponentKey, timeline, cancellationToken);
		if (team is not null)
		{
			ApplyTeam(analysis, timeline, team);
		}

		await MatchTimelineStorage.SaveAsync(fileStorage, analysis.TimelineObjectKey, timeline, analysis.ParserVersion, cancellationToken);
		dbContext.OpponentDemoAnalyses.Add(analysis);
		return analysis;
	}

	/// <summary>The opponent's team ("A"/"B") from their known SteamIDs; failing that, the team opposite our own roster (a
	/// demo of our match against them); null when neither tells.</summary>
	public static async Task<string?> DetectTeamAsync(
		IApplicationDbContext dbContext, string opponentKey, DemoTimeline timeline, CancellationToken cancellationToken)
	{
		var known = await KnownOpponentIdsAsync(dbContext, opponentKey, cancellationToken);
		if (OpponentSideDetector.Detect(timeline.Rounds, known) is { } team)
		{
			return team;
		}

		var roster = await MatchTimelineAttacher.LoadRosterSteamIdsAsync(dbContext, cancellationToken);
		return OpponentSideDetector.Detect(timeline.Rounds, roster) switch
		{
			OpponentSideDetector.TeamA => OpponentSideDetector.TeamB,
			OpponentSideDetector.TeamB => OpponentSideDetector.TeamA,
			_ => null
		};
	}

	/// <summary>Marks <paramref name="team"/> ("A"/"B") as the opponent and (re)extracts the tendency facts.</summary>
	public static void ApplyTeam(OpponentDemoAnalysis analysis, DemoTimeline timeline, string team)
	{
		var roster = team == OpponentSideDetector.TeamA ? analysis.TeamASteamIds : analysis.TeamBSteamIds;
		analysis.OpponentSteamIds = [.. roster];
		analysis.FactsJson = OpponentFactsSerializer.Serialize(OpponentFactsExtractor.Extract(timeline, roster.ToHashSet()));
	}

	/// <summary>Maps the row for the demo list.</summary>
	public static OpponentDemoDto ToDto(OpponentDemoAnalysis a)
	{
		string? team = a.OpponentSteamIds.Count == 0
			? null
			: a.OpponentSteamIds.Count(a.TeamASteamIds.Contains) >= a.OpponentSteamIds.Count(a.TeamBSteamIds.Contains)
				? OpponentSideDetector.TeamA
				: OpponentSideDetector.TeamB;

		return new OpponentDemoDto(
			a.Id,
			a.MapName?.ToString(),
			a.RawMapName,
			a.PlayedAtUtc,
			a.Source.ToString(),
			a.FaceitMatchId,
			a.RoundsCount,
			team is not null && a.FactsJson is not null,
			team,
			[
				new DemoTeamDto(OpponentSideDetector.TeamA, a.TeamASteamIds.Select(s => s.ToString()).ToList(), a.TeamANames),
				new DemoTeamDto(OpponentSideDetector.TeamB, a.TeamBSteamIds.Select(s => s.ToString()).ToList(), a.TeamBNames)
			],
			a.CreatedAtUtc);
	}

	#endregion
}
