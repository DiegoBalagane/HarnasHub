#region Usings

using HarnasHub.Application.Abstractions;

#endregion

namespace HarnasHub.Application.Common.Jobs;

/// <summary>Scoped <see cref="IJobProgress"/>: maps step fractions into percent windows, keeps the bar monotonic and forwards
/// to the job's sink — or does nothing when the scope isn't executing a job.</summary>
public sealed class JobProgress(JobExecutionContext context) : IJobProgress
{
	#region Public Fields

	/// <summary>Start of the window used when a handler parses a demo without declaring its own step.</summary>
	public const int DefaultStepFrom = 5;

	/// <summary>End of the default window; the rest is reserved for saving results.</summary>
	public const int DefaultStepTo = 95;

	/// <summary>Label of the default window.</summary>
	public const string DefaultStepStage = "Analiza demki";

	/// <summary>Label shown once the default step is complete.</summary>
	public const string SavingStage = "Zapisywanie wyników";

	#endregion

	#region Private Fields

	private readonly object _gate = new();
	private int _lastPercent;
	private string? _stage;
	private int _stepFrom = DefaultStepFrom;
	private int _stepTo = DefaultStepTo;
	private string _stepStage = DefaultStepStage;
	private bool _explicitStep;

	#endregion

	#region Public Methods

	/// <inheritdoc />
	public void Report(int percent, string? stage = null)
	{
		if (context.Sink is not { } sink)
		{
			return;
		}

		int published;
		string? publishedStage;
		lock (_gate)
		{
			_lastPercent = Math.Max(_lastPercent, Math.Clamp(percent, 0, 100));
			_stage = stage ?? _stage;
			published = _lastPercent;
			publishedStage = _stage;
		}

		sink.Publish(published, publishedStage);
	}

	/// <inheritdoc />
	public void BeginStep(int fromPercent, int toPercent, string stage)
	{
		lock (_gate)
		{
			_stepFrom = Math.Clamp(fromPercent, 0, 100);
			_stepTo = Math.Clamp(Math.Max(fromPercent, toPercent), 0, 100);
			_stepStage = stage;
			_explicitStep = true;
		}

		Report(fromPercent, stage);
	}

	/// <inheritdoc />
	public void ReportStep(double fraction)
	{
		int from, to;
		string stage;
		bool isDefault;
		lock (_gate)
		{
			(from, to, stage, isDefault) = (_stepFrom, _stepTo, _stepStage, !_explicitStep);
		}

		var clamped = double.IsNaN(fraction) ? 0 : Math.Clamp(fraction, 0, 1);
		if (isDefault && clamped >= 1)
		{
			Report(to, SavingStage);
			return;
		}

		Report(from + (int)Math.Floor((to - from) * clamped), stage);
	}

	#endregion
}
