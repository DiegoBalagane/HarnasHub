using ErrorOr;
using HarnasHub.Application.Features.Roster.Shared;
using MediatR;

namespace HarnasHub.Application.Features.Roster.GetRoster;

/// <summary>Returns every team member for the roster view.</summary>
public record GetRosterQuery : IRequest<ErrorOr<List<TeamMemberDto>>>;
