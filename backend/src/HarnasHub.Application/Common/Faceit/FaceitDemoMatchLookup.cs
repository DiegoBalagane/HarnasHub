#region Usings

using HarnasHub.Application.Abstractions;
using HarnasHub.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

#endregion

namespace HarnasHub.Application.Common.Faceit;

/// <summary>A FACEIT match found from a demo file name: the room, which faction is us (null when unknown) and, per faction,
/// the display name of an opponent already linked to (most of) that roster.</summary>
public sealed record FaceitDemoMatch(
	FaceitDemoFileReference Reference,
	FaceitMatchInfo Match,
	int? OurFactionIndex,
	IReadOnlyList<string?> LinkedOpponentNames);

/// <summary>Outcome of a lookup: the match when found, otherwise an optional subtle Polish note explaining why a FACEIT-named
/// demo got no prefill (no API key, FACEIT unavailable). Both null = the file name simply isn't a FACEIT demo name.</summary>
public sealed record FaceitDemoLookupResult(FaceitDemoMatch? Match, string? Note)
{
	#region Public Properties

	/// <summary>Nothing recognised, nothing to say.</summary>
	public static FaceitDemoLookupResult None { get; } = new(null, null);

	#endregion
}

/// <summary>Recognises a FACEIT match from a demo file name and loads its room through <see cref="IFaceitClient"/>. Never
/// fails the caller: without an API key or when FACEIT is down the demo is processed exactly as before, just without prefill.</summary>
public class FaceitDemoMatchLookup(IFaceitClient faceitClient, IApplicationDbContext dbContext, ILogger<FaceitDemoMatchLookup> logger)
{
	#region Public Fields

	/// <summary>Note when the API key isn't configured.</summary>
	public const string NotConfiguredNote = "Rozpoznano mecz FACEIT z nazwy pliku, ale integracja z FACEIT nie jest skonfigurowana — uzupełnij dane ręcznie.";

	/// <summary>Note when FACEIT couldn't be reached or doesn't know the match.</summary>
	public const string UnavailableNote = "Rozpoznano mecz FACEIT z nazwy pliku, ale nie udało się pobrać jego szczegółów — uzupełnij dane ręcznie.";

	#endregion

	#region Public Methods

	/// <summary>Looks the demo's FACEIT match up; see <see cref="FaceitDemoLookupResult"/> for the possible outcomes.</summary>
	public async Task<FaceitDemoLookupResult> FindAsync(string? fileName, CancellationToken cancellationToken)
	{
		if (FaceitDemoFileName.Parse(fileName) is not { } reference)
		{
			return FaceitDemoLookupResult.None;
		}

		if (!faceitClient.IsConfigured)
		{
			return new FaceitDemoLookupResult(null, NotConfiguredNote);
		}

		FaceitMatchInfo? match;
		try
		{
			match = await faceitClient.GetMatchAsync(reference.MatchId, cancellationToken);
		}
		catch (Exception ex) when (ex is not OperationCanceledException)
		{
			logger.LogWarning(ex, "Nie udało się pobrać meczu FACEIT {MatchId} rozpoznanego z nazwy demki", reference.MatchId);
			return new FaceitDemoLookupResult(null, UnavailableNote);
		}

		if (match is null || match.Factions.Count != 2)
		{
			return new FaceitDemoLookupResult(null, UnavailableNote);
		}

		var (ourSteamIds, ourFaceitIds) = await LoadOurIdsAsync(cancellationToken);
		var links = await dbContext.OpponentFaceitLinks.AsNoTracking().ToListAsync(cancellationToken);
		var linkedNames = match.Factions.Select(faction => LinkedOpponentName(faction, links)).ToList();

		return new FaceitDemoLookupResult(
			new FaceitDemoMatch(reference, match, FaceitFactionMatcher.OurFactionIndex(match.Factions, ourSteamIds, ourFaceitIds), linkedNames),
			null);
	}

	#endregion

	#region Private Methods

	/// <summary>Our roster's SteamID64s and the FACEIT ids cached for them.</summary>
	private async Task<(HashSet<string> SteamIds, HashSet<string> FaceitIds)> LoadOurIdsAsync(CancellationToken cancellationToken)
	{
		var steamIds = (await dbContext.Users
				.Where(u => u.SteamId64 != null && u.SteamId64 != "")
				.Select(u => u.SteamId64!)
				.ToListAsync(cancellationToken))
			.Select(s => s.Trim())
			.ToHashSet();
		var faceitIds = (await dbContext.FaceitPlayers
				.Where(p => p.SteamId64 != null && steamIds.Contains(p.SteamId64))
				.Select(p => p.Id)
				.ToListAsync(cancellationToken))
			.ToHashSet();
		return (steamIds, faceitIds);
	}

	/// <summary>Display name of the link covering most of the faction (at least 3 players, or the whole link when smaller).</summary>
	private static string? LinkedOpponentName(FaceitFactionInfo faction, List<OpponentFaceitLink> links)
	{
		var factionIds = faction.Players.Select(p => p.PlayerId).ToHashSet();
		return links
			.Select(link => (link, overlap: link.PlayerIds.Count(factionIds.Contains)))
			.Where(l => l.overlap > 0 && l.overlap >= Math.Min(3, l.link.PlayerIds.Count))
			.OrderByDescending(l => l.overlap)
			.Select(l => l.link.DisplayName)
			.FirstOrDefault();
	}

	#endregion
}
