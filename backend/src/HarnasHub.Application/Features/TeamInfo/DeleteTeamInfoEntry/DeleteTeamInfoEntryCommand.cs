using ErrorOr;
using MediatR;

namespace HarnasHub.Application.Features.TeamInfo.DeleteTeamInfoEntry;

/// <summary>Deletes an info entry. Coach/Manager only — enforced at the endpoint.</summary>
public record DeleteTeamInfoEntryCommand(Guid Id) : IRequest<ErrorOr<Success>>;
