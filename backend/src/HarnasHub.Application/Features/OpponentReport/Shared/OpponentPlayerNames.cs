#region Usings

using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Common.Demos;
using HarnasHub.Application.Features.OpponentReport.Tendencies;

#endregion

namespace HarnasHub.Application.Features.OpponentReport.Shared;

/// <summary>Display-time name resolution for opponent demo rosters and tendency player tables — stored names are untouched.</summary>
public static class OpponentPlayerNames
{
	#region Public Methods

	/// <summary>Returns the demos with every unnamed roster player replaced by a roster/FACEIT/fallback name.</summary>
	public static async Task<List<OpponentDemoDto>> ResolveDemosAsync(
		IApplicationDbContext dbContext,
		List<OpponentDemoDto> demos,
		CancellationToken cancellationToken)
	{
		var ids = demos
			.SelectMany(d => d.Teams)
			.SelectMany(t => t.SteamIds.Where((_, i) => PlayerNameResolver.IsUnnamed(NameAt(t, i))))
			.Select(s => long.TryParse(s, out var id) ? id : (long?)null)
			.OfType<long>()
			.ToList();

		if (ids.Count == 0)
		{
			return demos;
		}

		var known = await PlayerNameResolver.LoadKnownNamesAsync(dbContext, ids, cancellationToken);
		return demos.Select(d => d with { Teams = d.Teams.Select(t => ResolveTeam(t, known)).ToList() }).ToList();
	}

	/// <summary>Single-demo convenience over <see cref="ResolveDemosAsync"/>.</summary>
	public static async Task<OpponentDemoDto> ResolveDemoAsync(IApplicationDbContext dbContext, OpponentDemoDto demo, CancellationToken cancellationToken) =>
		(await ResolveDemosAsync(dbContext, [demo], cancellationToken))[0];

	/// <summary>Returns the per-map tendencies with unnamed players in the player table resolved.</summary>
	public static async Task<List<MapTendenciesDto>> ResolveTendenciesAsync(
		IApplicationDbContext dbContext,
		List<MapTendenciesDto> maps,
		CancellationToken cancellationToken)
	{
		var ids = maps
			.SelectMany(m => m.Players)
			.Where(p => PlayerNameResolver.IsUnnamed(p.Name))
			.Select(p => long.TryParse(p.SteamId64, out var id) ? id : (long?)null)
			.OfType<long>()
			.ToList();

		if (ids.Count == 0)
		{
			return maps;
		}

		var known = await PlayerNameResolver.LoadKnownNamesAsync(dbContext, ids, cancellationToken);
		return maps.Select(m => m with
		{
			Players = m.Players.Select(p => long.TryParse(p.SteamId64, out var id) ? p with { Name = PlayerNameResolver.Choose(id, p.Name, known) } : p).ToList()
		}).ToList();
	}

	#endregion

	#region Private Methods

	private static string? NameAt(DemoTeamDto team, int index) => index < team.Names.Count ? team.Names[index] : null;

	private static DemoTeamDto ResolveTeam(DemoTeamDto team, IReadOnlyDictionary<long, string> known)
	{
		var names = team.SteamIds
			.Select((s, i) => long.TryParse(s, out var id) ? PlayerNameResolver.Choose(id, NameAt(team, i), known) : NameAt(team, i) ?? s)
			.ToList();

		return team with { Names = names };
	}

	#endregion
}
