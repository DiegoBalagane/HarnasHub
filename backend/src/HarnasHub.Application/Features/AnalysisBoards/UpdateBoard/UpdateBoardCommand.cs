using ErrorOr;
using HarnasHub.Application.Features.AnalysisBoards.Shared;
using MediatR;

namespace HarnasHub.Application.Features.AnalysisBoards.UpdateBoard;

/// <summary>Edits a board's title/strokes/background in place — the same shape a fresh save produces, since the
/// canvas editor round-trips its whole current state on every save rather than diffing strokes. Coach/Manager only —
/// enforced at the endpoint. A non-null <paramref name="BackgroundImageObjectKey"/> that differs from what's stored
/// replaces the background (the caller uploaded a new image first); passing the same key or null-to-clear both work.</summary>
public record UpdateBoardCommand(
	Guid BoardId,
	string Title,
	string? BackgroundImageObjectKey,
	string StrokesJson) : IRequest<ErrorOr<AnalysisBoardDto>>;
