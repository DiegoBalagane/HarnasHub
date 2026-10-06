namespace HarnasHub.Core.Enums;

/// <summary>Lifecycle state of a <see cref="Entities.BackgroundJob"/>.</summary>
public enum BackgroundJobStatus
{
	Queued = 0,
	Running = 1,
	Succeeded = 2,
	Failed = 3
}
