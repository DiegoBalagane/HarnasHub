#region Usings

using ErrorOr;
using HarnasHub.Application.Features.OpponentReport.Shared;
using MediatR;

#endregion

namespace HarnasHub.Application.Features.OpponentReport.SetOpponentDemoTeam;

/// <summary>Sets which team of an analysed demo is the opponent ("A" started T, "B" started CT) — the fallback when automatic
/// detection failed, or a correction. Coach/Manager only, enforced at the endpoint.</summary>
public record SetOpponentDemoTeamCommand(Guid Id, string Team) : IRequest<ErrorOr<OpponentDemoDto>>;
