namespace HarnasHub.Core.Options;

/// <summary>Configuration of the in-process background job worker.</summary>
public class BackgroundJobOptions
{
	#region Public Fields

	/// <summary>Configuration section the options are bound from.</summary>
	public const string SectionName = "Jobs";

	#endregion

	#region Public Properties

	/// <summary>How many demo jobs (parsing is memory-heavy: a 300 MB demo needs several hundred MB) run at once.</summary>
	public int DemoConcurrency { get; set; } = 2;

	/// <summary>How many FACEIT API jobs (network-bound) run at once.</summary>
	public int FaceitConcurrency { get; set; } = 2;

	/// <summary>Finished jobs older than this many days are deleted on startup.</summary>
	public int RetentionDays { get; set; } = 7;

	#endregion
}
