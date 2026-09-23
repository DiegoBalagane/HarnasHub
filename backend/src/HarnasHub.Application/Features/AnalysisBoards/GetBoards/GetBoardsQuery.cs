using ErrorOr;
using HarnasHub.Application.Features.AnalysisBoards.Shared;
using HarnasHub.Core.Enums;
using MediatR;

namespace HarnasHub.Application.Features.AnalysisBoards.GetBoards;

/// <summary>Returns every saved analysis board, optionally narrowed to one map, most recently updated first.</summary>
public record GetBoardsQuery(MapName? MapName) : IRequest<ErrorOr<List<AnalysisBoardDto>>>;
