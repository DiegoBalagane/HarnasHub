#region Usings

using System.Text.Json;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;

#endregion

namespace HarnasHub.Application.Features.Jobs.Shared;

/// <summary>Response of every endpoint that starts a background job (202 Accepted).</summary>
public record JobAcceptedDto(Guid JobId);

/// <summary>Status of a background job; <paramref name="Result"/> is the raw JSON the synchronous endpoint used to return
/// (null until it succeeds), <paramref name="Error"/> the Polish failure message.</summary>
public record JobDto(
	Guid Id,
	string Kind,
	BackgroundJobStatus Status,
	int Progress,
	string? Stage,
	JsonElement? Result,
	string? Error,
	DateTime CreatedAtUtc,
	DateTime? StartedAtUtc,
	DateTime? FinishedAtUtc)
{
	#region Public Methods

	/// <summary>Maps a job row, embedding its stored result JSON as-is.</summary>
	public static JobDto From(BackgroundJob job)
	{
		JsonElement? result = null;
		if (job.ResultJson is { Length: > 0 } json)
		{
			using var document = JsonDocument.Parse(json);
			result = document.RootElement.Clone();
		}

		return new JobDto(
			job.Id, job.Kind, job.Status, job.Progress, job.Stage, result, job.ErrorMessage,
			job.CreatedAtUtc, job.StartedAtUtc, job.FinishedAtUtc);
	}

	#endregion
}
