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

	/// <summary>Player form and map comfort for their linked roster and our linked players over the cached window.</summary>
	public static IndividualFormResult Build(OpponentReportInput input)
	{
		var theirLines = IndividualGameLines.Build(input.Matches, input.TheirStats, input.TheirRoster);
		var ourLines = IndividualGameLines.Build(input.Matches, input.OurStats ?? [], input.OurRoster);
		var theirComfort = MapComfortCalculator.Calculate(theirLines);
		var ourComfort = MapComfortCalculator.Calculate(ourLines);

		var form = new IndividualFormDto(
			new TeamIndividualFormDto(
				PlayerFormCalculator.Calculate(theirLines, input.Link?.Players ?? []),
				MapComfortCalculator.ToDtos(theirComfort)),
			new TeamIndividualFormDto(
				PlayerFormCalculator.Calculate(ourLines, input.OurPlayers ?? []),
				MapComfortCalculator.ToDtos(ourComfort)));

		return new IndividualFormResult(form, theirComfort, ourComfort);
	}

	#endregion
}
