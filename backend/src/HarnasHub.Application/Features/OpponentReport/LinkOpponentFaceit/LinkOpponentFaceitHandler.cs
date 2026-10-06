using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.OpponentNotes.Shared;
using HarnasHub.Application.Features.OpponentReport.Shared;
using HarnasHub.Core.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HarnasHub.Application.Features.OpponentReport.LinkOpponentFaceit;

/// <summary>Handles <see cref="LinkOpponentFaceitCommand"/>: resolves the pasted source to FACEIT player ids, caches the players
/// and stores the link. The old report snapshot is dropped, since it describes the previous roster.</summary>
public class LinkOpponentFaceitHandler(
	IApplicationDbContext dbContext,
	IFaceitClient faceitClient,
	ICurrentUserService currentUser,
	ILogger<LinkOpponentFaceitHandler> logger)
	: IRequestHandler<LinkOpponentFaceitCommand, ErrorOr<OpponentFaceitLinkDto>>
{
	#region Public Methods

	public async Task<ErrorOr<OpponentFaceitLinkDto>> Handle(LinkOpponentFaceitCommand request, CancellationToken cancellationToken)
	{
		if (!faceitClient.IsConfigured)
		{
			return OpponentReportErrors.FaceitNotConfigured;
		}

		var source = FaceitLinkParser.Parse(request.Source);
		if (source is null)
		{
			return OpponentReportErrors.InvalidSource;
		}

		var key = OpponentNames.ToKey(request.OpponentName);
		var now = DateTime.UtcNow;

		try
		{
			var roster = source.Kind switch
			{
				FaceitLinkKind.Team => await ResolveTeamAsync(source.Id!, cancellationToken),
				FaceitLinkKind.Match => await ResolveMatchAsync(source.Id!, key, cancellationToken),
				_ => await ResolveNicknamesAsync(source.Nicknames, cancellationToken)
			};

			if (roster.IsError)
			{
				return roster.Errors;
			}

			if (roster.Value.Players.Count == 0)
			{
				return OpponentReportErrors.EmptyRoster;
			}

			var players = new List<FaceitPlayer>();
			foreach (var p in roster.Value.Players)
			{
				players.Add(await FaceitSync.UpsertPlayerAsync(dbContext, p.PlayerId, p.Nickname, p.SteamId64, p.Elo, p.SkillLevel, now, cancellationToken));
			}

			var link = await dbContext.OpponentFaceitLinks.FirstOrDefaultAsync(l => l.OpponentKey == key, cancellationToken);
			if (link is null)
			{
				link = new OpponentFaceitLink { Id = Guid.NewGuid(), OpponentKey = key };
				dbContext.OpponentFaceitLinks.Add(link);
			}

			link.DisplayName = request.OpponentName.Trim();
			link.FaceitTeamId = roster.Value.TeamId;
			link.PlayerIds = players.Select(p => p.Id).Distinct().ToList();
			link.LinkedByUserId = currentUser.UserId;
			link.LinkedAtUtc = now;
			link.LastSyncedAtUtc = null;

			await OpponentReportSnapshots.RemoveAsync(dbContext, key, cancellationToken);
			await dbContext.SaveChangesAsync(cancellationToken);

			return new OpponentFaceitLinkDto(
				link.DisplayName,
				link.FaceitTeamId,
				players.DistinctBy(p => p.Id).Select(p => new FaceitPlayerDto(p.Id, p.Nickname, p.Elo, p.SkillLevel)).ToList(),
				link.LinkedAtUtc,
				link.LastSyncedAtUtc);
		}
		catch (HttpRequestException ex)
		{
			logger.LogError(ex, "Nie udało się powiązać przeciwnika {Opponent} z FACEIT", request.OpponentName);
			return OpponentReportErrors.FaceitUnavailable;
		}
	}

	#endregion

	#region Private Methods

	/// <summary>The resolved roster; <paramref name="TeamId"/> is set only for team links.</summary>
	private sealed record ResolvedRoster(string? TeamId, List<FaceitPlayerInfo> Players);

	/// <summary>Every member of the FACEIT team.</summary>
	private async Task<ErrorOr<ResolvedRoster>> ResolveTeamAsync(string teamId, CancellationToken cancellationToken)
	{
		var team = await faceitClient.GetTeamAsync(teamId, cancellationToken);
		return team is null
			? OpponentReportErrors.TeamNotFound
			: new ResolvedRoster(team.TeamId, team.Members.Select(ToInfo).ToList());
	}

	/// <summary>The faction of the match room that isn't us — or, when we didn't play it, the one named like the opponent.</summary>
	private async Task<ErrorOr<ResolvedRoster>> ResolveMatchAsync(string matchId, string opponentKey, CancellationToken cancellationToken)
	{
		var match = await faceitClient.GetMatchAsync(matchId, cancellationToken);
		if (match is null || match.Factions.Count != 2)
		{
			return OpponentReportErrors.MatchNotFound;
		}

		var ourSteamIds = (await dbContext.Users
				.Where(u => u.SteamId64 != null && u.SteamId64 != "")
				.Select(u => u.SteamId64!)
				.ToListAsync(cancellationToken))
			.Select(s => s.Trim())
			.ToHashSet();
		var ourFaceitIds = (await dbContext.FaceitPlayers
				.Where(p => p.SteamId64 != null && ourSteamIds.Contains(p.SteamId64))
				.Select(p => p.Id)
				.ToListAsync(cancellationToken))
			.ToHashSet();

		bool IsOurs(FaceitFactionInfo faction) =>
			faction.Players.Any(p => ourFaceitIds.Contains(p.PlayerId) || (p.SteamId64 is not null && ourSteamIds.Contains(p.SteamId64)));

		var withUs = match.Factions.Where(IsOurs).ToList();
		var opponent = withUs.Count == 1
			? match.Factions.First(f => f != withUs[0])
			: withUs.Count == 0
				? SingleOrNull(match.Factions.Where(f => NameMatches(f.Name, opponentKey)).ToList())
				: null;

		return opponent is null
			? OpponentReportErrors.AmbiguousMatch
			: new ResolvedRoster(null, opponent.Players.Select(ToInfo).ToList());
	}

	/// <summary>Each nickname looked up on FACEIT; any miss fails the whole link so the coach can fix the typo.</summary>
	private async Task<ErrorOr<ResolvedRoster>> ResolveNicknamesAsync(List<string> nicknames, CancellationToken cancellationToken)
	{
		var found = new List<FaceitPlayerInfo>();
		var missing = new List<string>();
		foreach (var nickname in nicknames)
		{
			var player = await faceitClient.GetPlayerByNicknameAsync(nickname, cancellationToken);
			if (player is null)
			{
				missing.Add(nickname);
			}
			else
			{
				found.Add(player);
			}
		}

		return missing.Count > 0 ? OpponentReportErrors.PlayersNotFound(missing) : new ResolvedRoster(null, found);
	}

	/// <summary>A roster entry as a profile without elo.</summary>
	private static FaceitPlayerInfo ToInfo(FaceitPlayerRef player) =>
		new(player.PlayerId, player.Nickname, player.SteamId64, null, player.SkillLevel);

	/// <summary>The only faction of the list, or null when there are none or several.</summary>
	private static FaceitFactionInfo? SingleOrNull(List<FaceitFactionInfo> factions) => factions.Count == 1 ? factions[0] : null;

	/// <summary>Whether a faction name ("team_Nick" for pickup rooms, the team name otherwise) refers to the opponent.</summary>
	private static bool NameMatches(string factionName, string opponentKey)
	{
		var name = OpponentNames.ToKey(factionName);
		return name == opponentKey || (opponentKey.Length >= 3 && name.Contains(opponentKey, StringComparison.Ordinal));
	}

	#endregion
}
