using ErrorOr;
using HarnasHub.Application.Features.Results.Shared;
using MediatR;

namespace HarnasHub.Application.Features.Results.AnalyzeDemoFromStorage;

/// <summary>Analyses a demo previously uploaded straight to object storage via <c>PresignDemoUploadCommand</c> —
/// the large-file counterpart to <c>AnalyzeDemoCommand</c>, which takes the bytes directly over HTTP instead.
/// Coach/Manager only, enforced at the endpoint.</summary>
public record AnalyzeDemoFromStorageCommand(string ObjectKey) : IRequest<ErrorOr<AnalyzeDemoResultDto>>;
