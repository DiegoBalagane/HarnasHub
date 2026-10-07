using ErrorOr;
using HarnasHub.Application.Features.TeamInfo.Shared;
using MediatR;

namespace HarnasHub.Application.Features.TeamInfo.GetTeamInfo;

/// <summary>Returns every team info entry ordered by category and sort order; any team member may call it.</summary>
public record GetTeamInfoQuery : IRequest<ErrorOr<List<TeamInfoEntryDto>>>;
