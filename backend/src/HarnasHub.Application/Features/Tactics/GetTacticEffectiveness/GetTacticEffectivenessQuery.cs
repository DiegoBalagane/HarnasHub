#region Usings

using ErrorOr;
using HarnasHub.Application.Features.Tactics.Shared.Matching;
using HarnasHub.Core.Enums;
using MediatR;

#endregion

namespace HarnasHub.Application.Features.Tactics.GetTacticEffectiveness;

/// <summary>Per-tactic effectiveness (rounds played/won) of one map over its newest analysed matches.</summary>
public record GetTacticEffectivenessQuery(MapName Map) : IRequest<ErrorOr<TacticEffectivenessReportDto>>;
