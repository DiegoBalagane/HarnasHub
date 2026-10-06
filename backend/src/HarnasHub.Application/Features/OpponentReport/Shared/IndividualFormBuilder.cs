using HarnasHub.Core.Enums;

namespace HarnasHub.Application.Features.OpponentReport.Shared;

/// <summary>The individual-form part of a report: the DTO plus the raw per-map comfort of both rosters used for blending.</summary>
public record IndividualFormResult(
	IndividualFormDto Form,
	IReadOnlyDictionary<MapName, MapComfort> TheirComfort,
	IReadOnlyDictionary<MapName, MapComfort> OurComfort);

/// <summary>Builds <see cref="IndividualFormResult"/> from <see cref="OpponentReportInput"/> — pure, no I/O.</summary>
public static class IndividualFormBuilder
{
	#region Public Methods

	/// <summary>Player form and map comfort for their active lineup (<paramref name="theirActive"/>; the full linked roster still
	/// decides team vs solo) and our linked players over the cached window; comfort rates are recency-weighted.</summary>
	public static IndividualFormResult Build(OpponentReportInput input, IReadOnlySet<string> theirActive)
	{
		var theirLines = IndividualGameLines.Build(input.Matches, input.TheirStats, input.TheirRoster, theirActive);
		var ourLines = IndividualGameLines.Build(input.Matches, input.OurStats ?? [], input.OurRoster);
		var theirComfort = MapComfortCalculator.Calculate(theirLines, input.GeneratedAtUtc);
		var ourComfort = MapComfortCalculator.Calculate(ourLines, input.GeneratedAtUtc);
		var theirProfiles = (input.Link?.Players ?? []).Where(p => theirActive.Contains(p.PlayerId)).ToList();

		var form = new IndividualFormDto(
			new TeamIndividualFormDto(
				PlayerFormCalculator.Calculate(theirLines, theirProfiles),
				MapComfortCalculator.ToDtos(theirComfort)),
			new TeamIndividualFormDto(
				PlayerFormCalculator.Calculate(ourLines, input.OurPlayers ?? []),
				MapComfortCalculator.ToDtos(ourComfort)));

		return new IndividualFormResult(form, theirComfort, ourComfort);
	}

	#endregion
}
