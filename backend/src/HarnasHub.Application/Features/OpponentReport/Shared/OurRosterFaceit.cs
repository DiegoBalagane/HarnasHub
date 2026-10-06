using HarnasHub.Application.Abstractions;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.OpponentReport.Shared;

/// <summary>A roster player whose FACEIT account could not be found, with the Polish <paramref name="Reason"/> shown on the report's "our" tab.</summary>
public record UnresolvedRosterPlayerDto(Guid UserId, string DisplayName, string Reason);

/// <summary>Maps our roster to FACEIT accounts — by SteamID64 first, then by the manually set FACEIT nickname — and only
/// ever considers players that are shown in stats. Hidden players are never resolved, synced or reported.</summary>
public static class OurRosterFaceit
{
	#region Public Fields

	/// <summary>Reason: the player has neither a SteamID64 nor a FACEIT nickname.</summary>
	public const string NoIdentifiersReason = "brak SteamID i nicku FACEIT";

	/// <summary>Reason: the SteamID64 exists but FACEIT has no account linked to it, and no nickname is set.</summary>
	public const string SteamNotLinkedReason = "SteamID bez konta FACEIT — ustaw nick FACEIT w panelu admina";

	/// <summary>Reason: a FACEIT nickname is set but no such player was found.</summary>
	public const string NicknameNotFoundReason = "nie znaleziono nicku FACEIT";

	#endregion

	#region Public Methods

	/// <summary>Our players' FACEIT ids: each shown-in-stats user is resolved by SteamID64 first, then by <see cref="User.FaceitNickname"/>;
	/// cached profiles are reused while fresh, otherwise FACEIT is asked (SteamID lookup, then nickname lookup).</summary>
	public static async Task<List<string>> ResolveAsync(
		IApplicationDbContext dbContext,
		IFaceitClient client,
		DateTime nowUtc,
		CancellationToken cancellationToken)
	{
		var users = await LoadUsersAsync(dbContext, cancellationToken);
		var cache = await LoadCacheAsync(dbContext, users, cancellationToken);
		var ids = new List<string>();
		var lookedUp = new HashSet<string>();

		foreach (var user in users)
		{
			var player = Find(cache, user.SteamId, null);
			if (user.SteamId is not null && (player is null || IsStale(player, nowUtc)) && lookedUp.Add("s:" + user.SteamId))
			{
				var info = await client.GetPlayerBySteamIdAsync(user.SteamId, cancellationToken);
				if (info is not null)
				{
					player = await FaceitSync.UpsertPlayerAsync(dbContext, info.PlayerId, info.Nickname, user.SteamId, info.Elo, info.SkillLevel, nowUtc, cancellationToken);
					cache.Add(player);
				}
			}

			if (player is null && user.Nickname is not null)
			{
				player = Find(cache, null, user.Nickname);
				if ((player is null || IsStale(player, nowUtc)) && lookedUp.Add("n:" + user.Nickname.ToLowerInvariant()))
				{
					var info = await client.GetPlayerByNicknameAsync(user.Nickname, cancellationToken);
					if (info is not null)
					{
						player = await FaceitSync.UpsertPlayerAsync(dbContext, info.PlayerId, info.Nickname, info.SteamId64, info.Elo, info.SkillLevel, nowUtc, cancellationToken);
						cache.Add(player);
					}
				}
			}

			if (player is not null)
			{
				ids.Add(player.Id);
			}
		}

		await dbContext.SaveChangesAsync(cancellationToken);
		return ids.Distinct().ToList();
	}

	/// <summary>Cache-only view of our roster: the cached FACEIT players found for shown-in-stats users, plus the Main/Bench
	/// roster players that could not be matched, each with the reason. Makes no FACEIT calls.</summary>
	public static async Task<(List<FaceitPlayer> Players, List<UnresolvedRosterPlayerDto> Unresolved)> LoadCachedAsync(
		IApplicationDbContext dbContext,
		CancellationToken cancellationToken)
	{
		var users = await LoadUsersAsync(dbContext, cancellationToken);
		var cache = await LoadCacheAsync(dbContext, users, cancellationToken);
		var players = new List<FaceitPlayer>();
		var unresolved = new List<UnresolvedRosterPlayerDto>();

		foreach (var user in users)
		{
			var player = Find(cache, user.SteamId, null) ?? Find(cache, null, user.Nickname);
			if (player is not null)
			{
				players.Add(player);
			}
			else if (user.IsRosterPlayer)
			{
				unresolved.Add(new UnresolvedRosterPlayerDto(user.Id, user.Name, ReasonFor(user)));
			}
		}

		return (players.DistinctBy(p => p.Id).ToList(), unresolved);
	}

	#endregion

	#region Private Methods

	/// <summary>Why a user without a FACEIT account is unresolved.</summary>
	private static string ReasonFor(RosterUser user) =>
		user.Nickname is not null ? NicknameNotFoundReason
		: user.SteamId is not null ? SteamNotLinkedReason
		: NoIdentifiersReason;

	/// <summary>The cached player for a SteamID64 or (case-insensitive) nickname; null when neither is given or cached.</summary>
	private static FaceitPlayer? Find(List<FaceitPlayer> cache, string? steamId, string? nickname) =>
		steamId is not null
			? cache.FirstOrDefault(p => p.SteamId64 == steamId)
			: nickname is not null
				? cache.FirstOrDefault(p => string.Equals(p.Nickname, nickname, StringComparison.OrdinalIgnoreCase))
				: null;

	/// <summary>Whether the cached profile is older than <see cref="FaceitSync.ProfileMaxAge"/>.</summary>
	private static bool IsStale(FaceitPlayer player, DateTime nowUtc) => player.UpdatedAtUtc < nowUtc - FaceitSync.ProfileMaxAge;

	/// <summary>Shown-in-stats users that have a SteamID64 or a FACEIT nickname, or are Main/Bench roster players (to report them as unresolved).</summary>
	private static async Task<List<RosterUser>> LoadUsersAsync(IApplicationDbContext dbContext, CancellationToken cancellationToken)
	{
		var rows = await dbContext.Users.AsNoTracking()
			.Where(u => u.ShowInStats)
			.Select(u => new { u.Id, u.DisplayName, u.InGameNickname, u.SteamId64, u.FaceitNickname, u.AccessLevel, u.RosterSlot })
			.ToListAsync(cancellationToken);

		return rows
			.Select(u => new RosterUser(
				u.Id,
				u.InGameNickname ?? u.DisplayName,
				string.IsNullOrWhiteSpace(u.SteamId64) ? null : u.SteamId64.Trim(),
				string.IsNullOrWhiteSpace(u.FaceitNickname) ? null : u.FaceitNickname.Trim(),
				u.AccessLevel != AccessLevel.Guest && u.RosterSlot is RosterSlot.Main or RosterSlot.Bench))
			.Where(u => u.SteamId is not null || u.Nickname is not null || u.IsRosterPlayer)
			.ToList();
	}

	/// <summary>Cached FACEIT players matching any of the users' SteamID64s or nicknames (tracked, so a later upsert reuses them).</summary>
	private static async Task<List<FaceitPlayer>> LoadCacheAsync(
		IApplicationDbContext dbContext,
		List<RosterUser> users,
		CancellationToken cancellationToken)
	{
		var steamIds = users.Where(u => u.SteamId is not null).Select(u => u.SteamId!).Distinct().ToList();
		var nicknames = users.Where(u => u.Nickname is not null).Select(u => u.Nickname!.ToLower()).Distinct().ToList();

		return await dbContext.FaceitPlayers
			.Where(p => (p.SteamId64 != null && steamIds.Contains(p.SteamId64)) || nicknames.Contains(p.Nickname.ToLower()))
			.ToListAsync(cancellationToken);
	}

	#endregion

	#region Private Types

	/// <summary>The bits of a user the resolution needs; <paramref name="IsRosterPlayer"/> is a non-guest Main/Bench player.</summary>
	private record RosterUser(Guid Id, string Name, string? SteamId, string? Nickname, bool IsRosterPlayer);

	#endregion
}
