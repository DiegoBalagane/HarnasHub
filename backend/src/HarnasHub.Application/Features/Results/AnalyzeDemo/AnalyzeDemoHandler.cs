using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.Results.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.Results.AnalyzeDemo;

/// <summary>Handles <see cref="AnalyzeDemoCommand"/> by parsing the demo and splitting its round-1 sides into two
/// candidate "our team" groups — whichever the roster's SteamID64s overlap with (if either) is suggested, but the
/// coach picks the final one client-side, so this never has to guess wrong the way pure SteamID matching can when
/// nobody's SteamID64 is on file yet.</summary>
public class AnalyzeDemoHandler(IDemoParser demoParser, IApplicationDbContext dbContext)
	: IRequestHandler<AnalyzeDemoCommand, ErrorOr<AnalyzeDemoResultDto>>
{
	#region Public Methods

	public async Task<ErrorOr<AnalyzeDemoResultDto>> Handle(AnalyzeDemoCommand request, CancellationToken cancellationToken)
	{
		DemoParseResult parsed;

		try
		{
			parsed = await demoParser.ParseAsync(request.DemoStream, cancellationToken);
		}
		catch (Exception)
		{
			// Any parser failure (corrupt file, unsupported build, wrong file type) is a validation problem for the
			// caller, not a server error — the demo bytes themselves are untrusted input.
			return ResultErrors.InvalidDemoFile;
		}

		if (parsed.RoundsPlayed == 0 || parsed.Rounds.Count == 0 || parsed.Players.Count == 0)
		{
			return ResultErrors.InvalidDemoFile;
		}

		var firstRound = parsed.Rounds[0];
		var nameBySteamId = parsed.Players.ToDictionary(p => p.SteamId64, p => p.PlayerName);

		string NameOrId(long steamId) => nameBySteamId.TryGetValue(steamId, out var name) ? name : steamId.ToString();

		var teamA = BuildTeamPreview(parsed.Rounds, firstRound.TerroristSteamIds, NameOrId);
		var teamB = BuildTeamPreview(parsed.Rounds, firstRound.CounterTerroristSteamIds, NameOrId);

		var rosterSteamIds = await ResolveRosterSteamIdsAsync(cancellationToken);
		var suggestedTeam = firstRound.TerroristSteamIds.Any(rosterSteamIds.Contains)
			? "A"
			: firstRound.CounterTerroristSteamIds.Any(rosterSteamIds.Contains)
				? "B"
				: null;

		var players = parsed.Players
			.Select(p => new AnalyzedDemoPlayerDto(
				p.SteamId64.ToString(),
				p.PlayerName,
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
				p.MultiKillRounds.GetValueOrDefault(5)))
			.ToList();

		return new AnalyzeDemoResultDto(parsed.RoundsPlayed, parsed.MapName?.ToString(), teamA, teamB, suggestedTeam, players);
	}

	#endregion

	#region Private Methods

	/// <summary>A group's own would-be score, found by treating its round-1 members as "our roster" for the same
	/// per-round majority-side attribution <see cref="DemoScoreCalculator"/> uses for a real roster match — always
	/// resolves (never null) since a group's members are always present, by construction, in their own round 1.</summary>
	private static DemoTeamPreviewDto BuildTeamPreview(
		IReadOnlyList<DemoRoundResult> rounds,
		IReadOnlyList<long> roundOneSteamIds,
		Func<long, string> nameOrId)
	{
		var score = DemoScoreCalculator.Calculate(rounds, roundOneSteamIds.ToHashSet()) ?? (0, 0);
		return new DemoTeamPreviewDto(roundOneSteamIds.Select(nameOrId).ToList(), score.OurScore, score.OpponentScore);
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
