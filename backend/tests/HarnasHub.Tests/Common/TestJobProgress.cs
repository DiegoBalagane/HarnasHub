#region Usings

using HarnasHub.Application.Abstractions;

#endregion

namespace HarnasHub.Tests.Common;

/// <summary>Stub <see cref="IJobProgress"/> recording every report, so tests can check what a handler reported.</summary>
public class TestJobProgress : IJobProgress
{
	#region Public Properties

	/// <summary>Every absolute report and step start, in order.</summary>
	public List<(int Percent, string? Stage)> Reports { get; } = [];

	/// <summary>Every step fraction reported.</summary>
	public List<double> StepFractions { get; } = [];

	#endregion

	#region Public Methods

	/// <inheritdoc />
	public void Report(int percent, string? stage = null) => Reports.Add((percent, stage));

	/// <inheritdoc />
	public void BeginStep(int fromPercent, int toPercent, string stage) => Reports.Add((fromPercent, stage));

	/// <inheritdoc />
	public void ReportStep(double fraction) => StepFractions.Add(fraction);

	#endregion
}
