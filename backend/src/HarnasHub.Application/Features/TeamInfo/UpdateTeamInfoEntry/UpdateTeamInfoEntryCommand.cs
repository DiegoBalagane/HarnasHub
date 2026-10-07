using ErrorOr;
using HarnasHub.Application.Features.TeamInfo.Shared;
using MediatR;

namespace HarnasHub.Application.Features.TeamInfo.UpdateTeamInfoEntry;

/// <summary>Edits an info entry (moving it to another category appends it there). Coach/Manager only — enforced at the endpoint.</summary>
public record UpdateTeamInfoEntryCommand(Guid Id, string Category, string Title, string Value, bool IsSecret) : IRequest<ErrorOr<TeamInfoEntryDto>>;
