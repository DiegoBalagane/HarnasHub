#region Usings

using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.Jobs.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

#endregion

namespace HarnasHub.Application.Features.Jobs.GetJob;

/// <summary>Handles <see cref="GetJobQuery"/>: loads the job and lets only its requester or a Coach/Manager see it.</summary>
public class GetJobHandler(IApplicationDbContext dbContext, ICurrentUserService currentUser)
	: IRequestHandler<GetJobQuery, ErrorOr<JobDto>>
{
	#region Public Methods

	/// <inheritdoc />
	public async Task<ErrorOr<JobDto>> Handle(GetJobQuery request, CancellationToken cancellationToken)
	{
		var job = await dbContext.BackgroundJobs.AsNoTracking().FirstOrDefaultAsync(j => j.Id == request.JobId, cancellationToken);
		if (job is null)
		{
			return JobErrors.NotFound;
		}

		var isStaff = currentUser.IsCoach || currentUser.Role == "Manager";
		if (job.RequestedByUserId != currentUser.UserId && !isStaff)
		{
			return JobErrors.Forbidden;
		}

		return JobDto.From(job);
	}

	#endregion
}
