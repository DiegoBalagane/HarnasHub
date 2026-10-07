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

	/// <summary>Feeds each map's FACEIT numbers from <paramref name="report"/> (their team games, our FACEIT team games, our solo
	/// prior) into the matching veto input.</summary>
	public static List<MapVetoInput> ApplyFaceit(IEnumerable<MapVetoInput> inputs, OpponentReportDto report)
	{
		var rows = report.Maps.ToDictionary(m => m.MapName, StringComparer.OrdinalIgnoreCase);
		return inputs
			.Select(input => rows.TryGetValue(input.MapName.ToString(), out var row)
				? WithFaceit(
					input,
					row.TheirGames,
					row.TheirWins,
					row.TheirSmoothedWinRate is { } rate ? rate / 100 : MapAdvantage.SmoothedWinRate(row.TheirWins, row.TheirGames),
					row.OurFaceitWins.HasValue ? row.OurFaceitGames : 0,
					row.OurFaceitWins ?? 0,
					row.OurSoloPrior,
					row.OurPlaysIndividually ?? false)
				: input)
			.ToList();
	}

	/// <summary>Adds the FACEIT numbers to a veto input; <paramref name="theirWinRate"/> is their recency-weighted smoothed win rate
	/// (0–1). Older snapshots without our FACEIT wins pass 0 games, so only the internal record counts; older snapshots without
	/// <paramref name="ourPlaysIndividually"/> count only team games and pool status towards familiarity.</summary>
	public static MapVetoInput WithFaceit(
		MapVetoInput input,
		int theirGames,
		int theirWins,
		double theirWinRate,
		int ourFaceitGames,
		int ourFaceitWins,
		double? ourSoloPrior,
		bool ourPlaysIndividually = false) =>
		input with
		{
			TheirGames = theirGames,
			TheirWins = theirWins,
			TheirWinRate = theirGames == 0 ? null : theirWinRate,
			OurFaceitGames = ourFaceitGames,
			OurFaceitWins = ourFaceitWins,
			OurSoloPrior = ourSoloPrior,
			OurPlaysIndividually = ourPlaysIndividually
		};

	#endregion
}
