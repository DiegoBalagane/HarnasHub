using ErrorOr;
using HarnasHub.Application.Features.AnalysisBoards.Shared;
using MediatR;

namespace HarnasHub.Application.Features.AnalysisBoards.PresignBoardImageUpload;

/// <summary>Requests a time-limited URL the browser can upload a board's background screenshot to directly —
/// same presign-then-PUT pattern as a demo upload. Coach/Manager only, enforced at the endpoint.</summary>
public record PresignBoardImageUploadCommand : IRequest<ErrorOr<PresignBoardImageUploadResultDto>>;
