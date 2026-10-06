namespace HarnasHub.Application.Abstractions;

/// <summary>Lets a handler running as a background job report its progress; outside a job (plain HTTP request, background
/// service, unit test) every call is a no-op, so handlers can report unconditionally.</summary>
public interface IJobProgress
{
	/// <summary>Reports the overall progress in percent (0–100, never moving backwards) and optionally a new Polish stage label.</summary>
	void Report(int percent, string? stage = null);

	/// <summary>Declares the window [<paramref name="fromPercent"/>, <paramref name="toPercent"/>] that following
	/// <see cref="ReportStep"/> calls map into, labelled <paramref name="stage"/> — e.g. one demo out of several.</summary>
	void BeginStep(int fromPercent, int toPercent, string stage);

	/// <summary>Reports how far (0–1) the current step is; without an explicit step the default demo-analysis window is used.</summary>
	void ReportStep(double fraction);
}
