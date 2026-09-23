using ErrorOr;
using MediatR;

namespace HarnasHub.Application.Features.AnalysisBoards.DeleteBoard;

/// <summary>Deletes an analysis board and its uploaded background image, if any. Coach/Manager only.</summary>
public record DeleteBoardCommand(Guid BoardId) : IRequest<ErrorOr<Success>>;
