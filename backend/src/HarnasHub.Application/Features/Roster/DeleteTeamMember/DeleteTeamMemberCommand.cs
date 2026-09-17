using ErrorOr;
using MediatR;

namespace HarnasHub.Application.Features.Roster.DeleteTeamMember;

/// <summary>Permanently deletes a team member's account and personal data. Manager only — enforced at the endpoint.</summary>
public record DeleteTeamMemberCommand(Guid UserId) : IRequest<ErrorOr<Success>>;
