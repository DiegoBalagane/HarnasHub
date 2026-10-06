using ErrorOr;
using HarnasHub.Application.Features.OpponentNotes.Shared;
using MediatR;

namespace HarnasHub.Application.Features.OpponentNotes.GetOpponents;

/// <summary>Lists every opponent the team has notes, results or scheduled games for, with the head-to-head record;
/// hidden opponents are left out unless <paramref name="IncludeHidden"/> is set.</summary>
public record GetOpponentsQuery(bool IncludeHidden = false) : IRequest<ErrorOr<List<OpponentSummaryDto>>>;
