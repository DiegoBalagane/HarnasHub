using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.MapPool.Shared;
using HarnasHub.Core.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.MapPool.GetMapPool;

/// <summary>Handles <see cref="GetMapPoolQuery"/>. Every <see cref="MapName"/> is always listed, even with no games or
/// status, so the page shows the whole pool rather than only what was played.</summary>
public class GetMapPoolHandler(IApplicationDbContext dbContext)
	: IRequestHandler<GetMapPoolQuery, ErrorOr<List<MapPoolMapDto>>>
{
	#region Private Fields

	private const int RecentFormLength = 5;

	#endregion

	#region Public Methods

	public async Task<ErrorOr<List<MapPoolMapDto>>> Handle(GetMapPoolQuery request, CancellationToken cancellationToken)
	{
		var entries = await dbContext.MapPoolEntries.ToDictionaryAsync(e => e.MapName, cancellationToken);

		var matchQuery = dbContext.MatchResults.Where(m => m.MapName != null);
		if (request.Category is { } category)
		{
			matchQuery = matchQuery.Where(m => m.Category == category);
		}

		var matches = await matchQuery
			.OrderByDescending(m => m.PlayedAtUtc)
			.Select(m => new { m.MapName, m.OurScore, m.OpponentScore, m.PlayedAtUtc })
			.ToListAsync(cancellationToken);

		// MatchResult.MapName is free text (manual entry or the demo parser's enum name) — match it to the pool leniently.
		var matchesByMap = matches
			.Select(m => (Parsed: MapNameParser.Parse(m.MapName), Match: m))
			.Where(x => x.Parsed.HasValue)
			.ToLookup(x => x.Parsed!.Value, x => x.Match);

		var tacticCounts = await dbContext.Tactics
			.GroupBy(t => t.MapName)
			.Select(g => new { MapName = g.Key, Count = g.Count() })
			.ToDictionaryAsync(x => x.MapName, x => x.Count, cancellationToken);

		return Enum.GetValues<MapName>()
			.Select(map =>
			{
				var played = matchesByMap[map].ToList();
				var outcomes = played.Select(m => Outcome(m.OurScore, m.OpponentScore)).ToList();
				var wins = outcomes.Count(o => o == "W");
				var losses = outcomes.Count(o => o == "L");
				var draws = outcomes.Count(o => o == "D");
				entries.TryGetValue(map, out var entry);

				var dto = new MapPoolMapDto(
					map.ToString(),
					entry?.Status.ToString(),
					entry?.Note,
					wins,
					losses,
					draws,
					played.Count == 0 ? null : Math.Round((wins + draws / 2.0) / played.Count * 100, 1),
					outcomes.Take(RecentFormLength).ToList(),
					played.Count == 0 ? null : played[0].PlayedAtUtc,
					tacticCounts.GetValueOrDefault(map));

				return (Rank: StatusRank(entry?.Status), Dto: dto);
			})
			.OrderBy(x => x.Rank)
			.ThenByDescending(x => x.Dto.Wins + x.Dto.Losses + x.Dto.Draws)
			.ThenBy(x => x.Dto.MapName)
			.Select(x => x.Dto)
			.ToList();
	}

	#endregion

	#region Private Methods

	/// <summary>"W"/"L"/"D" for one score line.</summary>
	private static string Outcome(int ourScore, int opponentScore) =>
		ourScore > opponentScore ? "W" : ourScore < opponentScore ? "L" : "D";

	/// <summary>Sort order of the pool: comfort picks first, unclassified maps before bans, bans last.</summary>
	private static int StatusRank(MapPoolStatus? status) => status switch
	{
		MapPoolStatus.Core => 0,
		MapPoolStatus.Playable => 1,
		MapPoolStatus.Learning => 2,
		null => 3,
		MapPoolStatus.Ban => 4,
		_ => 5
	};

	#endregion
}
