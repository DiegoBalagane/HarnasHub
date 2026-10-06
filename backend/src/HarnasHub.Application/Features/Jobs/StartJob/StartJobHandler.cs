#region Usings

using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Common.Jobs;
using HarnasHub.Application.Features.Jobs.Shared;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using MediatR;

#endregion

namespace HarnasHub.Application.Features.Jobs.StartJob;

/// <summary>Handles <see cref="StartJobCommand"/>: validates the wrapped request up front (so bad input is still a 400),
/// persists the job as Queued — before enqueueing, so the worker always finds the row — and returns its id.</summary>
public class StartJobHandler(
	IApplicationDbContext dbContext,
	IJobQueue jobQueue,
	ICurrentUserService currentUser,
	IServiceProvider services) : IRequestHandler<StartJobCommand, ErrorOr<JobAcceptedDto>>
{
	#region Public Methods

	/// <inheritdoc />
	public async Task<ErrorOr<JobAcceptedDto>> Handle(StartJobCommand request, CancellationToken cancellationToken)
	{
		if (JobRegistry.ByRequestType(request.Request.GetType()) is not { } definition)
		{
			return JobErrors.UnknownKind;
		}

		var errors = await definition.ValidateAsync(request.Request, services, cancellationToken);
		if (errors.Count > 0)
		{
			return errors;
		}

		var job = new BackgroundJob
		{
			Id = Guid.NewGuid(),
			Kind = definition.Kind,
			Status = BackgroundJobStatus.Queued,
			Progress = 0,
			Stage = "W kolejce",
			RequestedByUserId = currentUser.UserId,
			PayloadJson = definition.SerializePayload(request.Request),
			CreatedAtUtc = DateTime.UtcNow
		};

		dbContext.BackgroundJobs.Add(job);
		await dbContext.SaveChangesAsync(cancellationToken);
		jobQueue.Enqueue(job.Id, definition.Lane);

		return new JobAcceptedDto(job.Id);
	}

	#endregion
}
