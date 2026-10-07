using ErrorOr;
using HarnasHub.Application.Features.TeamInfo.Shared;
using MediatR;

namespace HarnasHub.Application.Features.TeamInfo.CreateTeamInfoEntry;

/// <summary>Adds an info entry at the end of its category. Coach/Manager only — enforced at the endpoint.</summary>
public record CreateTeamInfoEntryCommand(string Category, string Title, string Value, bool IsSecret) : IRequest<ErrorOr<TeamInfoEntryDto>>;
