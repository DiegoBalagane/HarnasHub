using System.Text.Json;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.Veto.Shared;
using HarnasHub.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.OpponentReport.Shared;

/// <summary>Stores and reads the one <see cref="OpponentReportSnapshot"/> per opponent, and turns a report into veto inputs.</summary>
public static class OpponentReportSnapshots
{
	#region Private Fields

	private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

	#endregion

	#region Public Methods

	/// <summary>The latest report for the opponent key, or null when none was generated (or the stored JSON no longer fits the DTO).</summary>
	public static async Task<OpponentReportDto?> LoadAsync(IApplicationDbContext dbContext, string opponentKey, CancellationToken cancellationToken)
	{
		var json = await dbContext.OpponentReportSnapshots
			.Where(s => s.OpponentKey == opponentKey)
			.Select(s => s.ReportJson)
			.FirstOrDefaultAsync(cancellationToken);

		if (json is null)
		{
			return null;
		}

		try
		{
			return JsonSerializer.Deserialize<OpponentReportDto>(json, JsonOptions);
		}
		catch (JsonException)
		{
			return null;
		}
	}

	/// <summary>Replaces the opponent's snapshot with <paramref name="report"/>; the caller saves changes.</summary>
	public static async Task SaveAsync(IApplicationDbContext dbContext, string opponentKey, OpponentReportDto report, CancellationToken cancellationToken)
	{
		var snapshot = await dbContext.OpponentReportSnapshots.FirstOrDefaultAsync(s => s.OpponentKey == opponentKey, cancellationToken);
		if (snapshot is null)
		{
			snapshot = new OpponentReportSnapshot { Id = Guid.NewGuid(), OpponentKey = opponentKey };
			dbContext.OpponentReportSnapshots.Add(snapshot);
		}

		snapshot.GeneratedAtUtc = report.GeneratedAtUtc;
		snapshot.DataSyncedAtUtc = report.DataSyncedAtUtc;
		snapshot.ReportJson = JsonSerializer.Serialize(report, JsonOptions);
	}

	/// <summary>Drops the opponent's snapshot (e.g. after relinking, when it describes the wrong roster); the caller saves changes.</summary>
	public static async Task RemoveAsync(IApplicationDbContext dbContext, string opponentKey, CancellationToken cancellationToken)
	{
		var snapshot = await dbContext.OpponentReportSnapshots.FirstOrDefaultAsync(s => s.OpponentKey == opponentKey, cancellationToken);
		if (snapshot is not null)
		{
			dbContext.OpponentReportSnapshots.Remove(snapshot);
		}
	}

	/// <summary>Feeds each map's FACEIT advantage from <paramref name="report"/> into the matching veto input.</summary>
	public static List<MapVetoInput> ApplyFaceitAdvantage(IEnumerable<MapVetoInput> inputs, OpponentReportDto report)
	{
		var rows = report.Maps.ToDictionary(m => m.MapName, StringComparer.OrdinalIgnoreCase);
		return inputs
			.Select(input => rows.TryGetValue(input.MapName.ToString(), out var row)
				? WithFaceit(input, row.TheirGames, row.OurGames, row.Advantage / 100)
				: input)
			.ToList();
	}

	/// <summary>Adds the advantage to a veto input — only when the opponent has team games on the map, since an edge
	/// "over them" with no data about them is just our own record again.</summary>
	public static MapVetoInput WithFaceit(MapVetoInput input, int theirGames, int ourGames, double advantage) =>
		theirGames == 0 ? input : input with { FaceitAdvantage = advantage, FaceitMinGames = Math.Min(theirGames, ourGames) };

	#endregion
}
