using ErrorOr;
using HarnasHub.Application.Features.Results.Shared;
using MediatR;

namespace HarnasHub.Application.Features.Results.AnalyzeDemo;

/// <summary>Parses an uploaded demo (never persisted) and previews what logging a result from it would look like —
/// nothing is saved until the coach reviews the preview and submits <c>AddResultCommand</c>. Coach/Manager only,
/// enforced at the endpoint. <paramref name="FileName"/> (the original file name, optional) lets a FACEIT demo prefill the form.</summary>
public record AnalyzeDemoCommand(Stream DemoStream, string? FileName = null) : IRequest<ErrorOr<AnalyzeDemoResultDto>>;
