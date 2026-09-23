using ErrorOr;
using MediatR;

namespace HarnasHub.Application.Features.Tasks.SubmitTaskForReview;

/// <summary>Marks a task assigned to the current user as ready for the coach's review.</summary>
public record SubmitTaskForReviewCommand(Guid TaskId) : IRequest<ErrorOr<Success>>;
