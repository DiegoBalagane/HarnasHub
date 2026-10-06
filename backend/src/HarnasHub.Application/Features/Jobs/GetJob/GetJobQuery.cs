#region Usings

using ErrorOr;
using HarnasHub.Application.Features.Jobs.Shared;
using MediatR;

#endregion

namespace HarnasHub.Application.Features.Jobs.GetJob;

/// <summary>Status, progress and (once done) result of one background job — readable by whoever started it and by Coach/Manager.</summary>
public record GetJobQuery(Guid JobId) : IRequest<ErrorOr<JobDto>>;
