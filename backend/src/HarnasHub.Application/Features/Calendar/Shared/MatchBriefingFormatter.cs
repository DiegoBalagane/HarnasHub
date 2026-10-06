#region Usings

using System.Globalization;
using System.Text;
using HarnasHub.Application.Features.OpponentReport.Shared;

#endregion

namespace HarnasHub.Application.Features.Calendar.Shared;

/// <summary>Turns an opponent report into the short Discord briefing posted before a match (top insights, BO1 veto, players to watch, link).</summary>
public static class MatchBriefingFormatter
{
	#region Public Fields

	/// <summary>Discord's hard limit for a single message.</summary>
	public const int MaxMessageLength = 2000;

	#endregion

	#region Public Methods

	/// <summary>The briefing text, never longer than <see cref="MaxMessageLength"/>; <paramref name="reportUrl"/> may be null.</summary>
	public static string Format(string eventTitle, string opponentName, OpponentReportDto report, string? reportUrl)
	{
		var link = string.IsNullOrWhiteSpace(reportUrl) ? null : $"\n🔗 Pełny raport: {reportUrl}";
		var sb = new StringBuilder();
		sb.Append($"📋 **Odprawa przed meczem: {eventTitle}** (vs {opponentName})");

		var insights = report.Insights
			.OrderBy(i => SeverityRank(i.Severity))
			.Take(3)
			.ToList();
		if (insights.Count > 0)
		{
			sb.Append("\n**Najważniejsze:**");
			foreach (var insight in insights)
			{
				sb.Append($"\n• {insight.Text}");
			}
		}

		if (VetoLine(report) is { } veto)
		{
			sb.Append($"\n**Veto BO1:** {veto}");
		}

		var players = PlayersToWatch(report);
		if (players.Count > 0)
		{
			sb.Append($"\n**Uważaj na:** {string.Join(", ", players)}");
		}

		return Truncate(sb.ToString(), link);
	}

	/// <summary>One line from the BO1 plan, e.g. "my ban Nuke → oni ban Mirage → decider Inferno"; null without a BO1 plan.</summary>
	public static string? VetoLine(OpponentReportDto report)
	{
		var plan = report.VetoPlans.FirstOrDefault(p => string.Equals(p.Format, "Bo1", StringComparison.OrdinalIgnoreCase));
		if (plan is null || plan.Steps.Count == 0)
		{
			return null;
		}

		return string.Join(" → ", plan.Steps.OrderBy(s => s.Order).Select(Step));
	}

	/// <summary>Up to three opponent players with the best K/D across maps, formatted "Nick (K/D 1.31, ADR 82)".</summary>
	public static List<string> PlayersToWatch(OpponentReportDto report) =>
		report.PlayersToWatch
			.SelectMany(m => m.Players)
			.GroupBy(p => p.PlayerId)
			.Select(g => g.OrderByDescending(p => p.KdRatio).First())
			.OrderByDescending(p => p.KdRatio)
			.Take(3)
			.Select(p => p.Adr is { } adr
				? $"{p.Nickname} (K/D {p.KdRatio.ToString("0.00", CultureInfo.InvariantCulture)}, ADR {adr.ToString("0", CultureInfo.InvariantCulture)})"
				: $"{p.Nickname} (K/D {p.KdRatio.ToString("0.00", CultureInfo.InvariantCulture)})")
			.ToList();

	#endregion

	#region Private Methods

	private static int SeverityRank(string severity) => severity switch
	{
		"High" => 0,
		"Warning" => 1,
		_ => 2
	};

	private static string Step(VetoPlanStepDto step)
	{
		var who = string.Equals(step.Actor, "Us", StringComparison.OrdinalIgnoreCase) ? "my" : "oni";
		return string.Equals(step.Action, "Decider", StringComparison.OrdinalIgnoreCase)
			? $"decider {step.MapName}"
			: $"{who} {step.Action.ToLowerInvariant()} {step.MapName}";
	}

	private static string Truncate(string body, string? link)
	{
		var linkLength = link?.Length ?? 0;
		if (body.Length + linkLength > MaxMessageLength)
		{
			body = body[..(MaxMessageLength - linkLength - 1)] + "…";
		}

		return body + link;
	}

	#endregion
}
