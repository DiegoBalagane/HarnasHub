using HarnasHub.Application.Features.OpponentReport.Shared;
using HarnasHub.Core.Enums;

namespace HarnasHub.Tests.Application.Features.OpponentReport;

/// <summary>Builders for <see cref="IndividualGameLine"/>s shared by the individual-form tests.</summary>
public static class IndividualLineFactory
{
	#region Public Fields

	/// <summary>Reference "now" of the generated lines.</summary>
	public static readonly DateTime Now = new(2026, 10, 1, 12, 0, 0, DateTimeKind.Utc);

	#endregion

	#region Public Methods

	/// <summary><paramref name="count"/> lines of one player on one map, newest first (the newest <paramref name="daysAgo"/> days ago,
	/// one day apart); the first <paramref name="wins"/> of them are wins.</summary>
	public static IEnumerable<IndividualGameLine> Lines(
		string playerId,
		MapName? map,
		int count,
		int wins = 0,
		bool team = false,
		int kills = 20,
		int deaths = 20,
		int daysAgo = 1) =>
		Enumerable.Range(0, count).Select(i => new IndividualGameLine(
			playerId,
			$"nick-{playerId}",
			map,
			Now.AddDays(-daysAgo - i),
			i < wins,
			team,
			kills,
			deaths,
			80,
			50));

	#endregion
}
