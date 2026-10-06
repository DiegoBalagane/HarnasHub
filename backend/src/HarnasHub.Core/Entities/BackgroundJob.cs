#region Usings

using HarnasHub.Core.Enums;

#endregion

namespace HarnasHub.Core.Entities;

/// <summary>A long operation (demo analysis, FACEIT sync, demo download…) queued by a user and executed by the in-process
/// background worker; the row carries its progress and, once finished, its JSON result or a Polish error message.</summary>
public class BackgroundJob
{
	#region Public Properties

	/// <summary>Primary key, also the id clients poll and subscribe to.</summary>
	public Guid Id { get; set; }

	/// <summary>Which operation this is (a <c>JobKinds</c> constant), used to pick the handler.</summary>
	public string Kind { get; set; } = string.Empty;

	/// <summary>Where the job is in its lifecycle.</summary>
	public BackgroundJobStatus Status { get; set; }

	/// <summary>Completion percentage, 0–100.</summary>
	public int Progress { get; set; }

	/// <summary>Optional Polish label of the current step (e.g. "Analiza demki").</summary>
	public string? Stage { get; set; }

	/// <summary>The user who started the job — only they and Coach/Manager may read it.</summary>
	public Guid RequestedByUserId { get; set; }

	/// <summary>The serialized request the job executes.</summary>
	public string PayloadJson { get; set; } = string.Empty;

	/// <summary>The serialized result (same shape the synchronous endpoint used to return), set on success.</summary>
	public string? ResultJson { get; set; }

	/// <summary>Polish error message, set on failure.</summary>
	public string? ErrorMessage { get; set; }

	/// <summary>When the job was queued.</summary>
	public DateTime CreatedAtUtc { get; set; }

	/// <summary>When a worker picked it up.</summary>
	public DateTime? StartedAtUtc { get; set; }

	/// <summary>When it succeeded or failed.</summary>
	public DateTime? FinishedAtUtc { get; set; }

	#endregion
}
