#region Usings

using System.Text.Json;
using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

#endregion

namespace HarnasHub.Application.Common.Jobs;

/// <summary>Describes one job kind: which MediatR request it executes, on which lane, how long it may run and how its
/// payload/result are (de)serialized — the worker stays generic and dispatches every kind the same way.</summary>
public interface IJobDefinition
{
	#region Public Properties

	/// <summary>Stable kind identifier stored on the job row.</summary>
	string Kind { get; }

	/// <summary>The MediatR request type the job executes.</summary>
	Type RequestType { get; }

	/// <summary>Worker pool the kind runs on.</summary>
	JobLane Lane { get; }

	/// <summary>Polish label shown from the moment a worker picks the job up.</summary>
	string InitialStage { get; }

	/// <summary>Hard time limit of one execution.</summary>
	TimeSpan Timeout { get; }

	#endregion

	#region Public Methods

	/// <summary>Runs the request's FluentValidation validator (if any) up front, so invalid input is a 400 instead of a failed job.</summary>
	Task<List<Error>> ValidateAsync(object request, IServiceProvider services, CancellationToken cancellationToken);

	/// <summary>Serializes the request into the job's payload.</summary>
	string SerializePayload(object request);

	/// <summary>Deserializes the payload, sends it through MediatR and turns the <c>ErrorOr</c> into a <see cref="JobOutcome"/>.</summary>
	Task<JobOutcome> ExecuteAsync(ISender sender, string payloadJson, CancellationToken cancellationToken);

	#endregion
}

/// <summary>Typed <see cref="IJobDefinition"/> for a request returning <c>ErrorOr&lt;TResult&gt;</c>.</summary>
public sealed class JobDefinition<TRequest, TResult>(string kind, JobLane lane, string initialStage, TimeSpan timeout) : IJobDefinition
	where TRequest : IRequest<ErrorOr<TResult>>
{
	#region Public Properties

	/// <inheritdoc />
	public string Kind { get; } = kind;

	/// <inheritdoc />
	public Type RequestType => typeof(TRequest);

	/// <inheritdoc />
	public JobLane Lane { get; } = lane;

	/// <inheritdoc />
	public string InitialStage { get; } = initialStage;

	/// <inheritdoc />
	public TimeSpan Timeout { get; } = timeout;

	#endregion

	#region Public Methods

	/// <inheritdoc />
	public async Task<List<Error>> ValidateAsync(object request, IServiceProvider services, CancellationToken cancellationToken)
	{
		var validator = services.GetService<IValidator<TRequest>>();
		if (validator is null)
		{
			return [];
		}

		var result = await validator.ValidateAsync((TRequest)request, cancellationToken);
		return result.Errors.Select(failure => Error.Validation(failure.PropertyName, failure.ErrorMessage)).ToList();
	}

	/// <inheritdoc />
	public string SerializePayload(object request) => JsonSerializer.Serialize((TRequest)request, JobJson.Options);

	/// <inheritdoc />
	public async Task<JobOutcome> ExecuteAsync(ISender sender, string payloadJson, CancellationToken cancellationToken)
	{
		var request = JsonSerializer.Deserialize<TRequest>(payloadJson, JobJson.Options)
			?? throw new InvalidOperationException($"Pusty ładunek zadania {Kind}.");

		var result = await sender.Send(request, cancellationToken);
		return result.IsError
			? JobOutcome.Failure(result.FirstError.Description)
			: JobOutcome.Success(JsonSerializer.Serialize(result.Value, JobJson.Options));
	}

	#endregion
}
