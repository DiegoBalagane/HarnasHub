using ErrorOr;
using MediatR;

namespace HarnasHub.Application.Features.Results.DeleteResult;

/// <summary>Deletes a logged result and every stat line saved against it. Coach/Manager only — enforced at the endpoint.</summary>
public record DeleteResultCommand(Guid MatchResultId) : IRequest<ErrorOr<Success>>;
