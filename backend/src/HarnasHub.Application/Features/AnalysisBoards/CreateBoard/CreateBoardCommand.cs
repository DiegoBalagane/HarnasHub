using ErrorOr;
using HarnasHub.Application.Features.AnalysisBoards.Shared;
using HarnasHub.Core.Enums;
using MediatR;

namespace HarnasHub.Application.Features.AnalysisBoards.CreateBoard;

/// <summary>Saves a new analysis board. Coach/Manager only — enforced at the endpoint.
/// <paramref name="BackgroundImageObjectKey"/> is the object key returned by an earlier presign-and-upload round trip;
/// null draws over the built-in radar for <paramref name="MapName"/> instead.</summary>
public record CreateBoardCommand(
	MapName MapName,
	string Title,
	string? BackgroundImageObjectKey,
	string StrokesJson) : IRequest<ErrorOr<AnalysisBoardDto>>;
