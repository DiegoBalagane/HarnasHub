#region Usings

using ErrorOr;
using HarnasHub.Application.Features.Results.Shared;
using MediatR;

#endregion

namespace HarnasHub.Application.Features.Results.AnalyzeUploadedDemo;

/// <summary>Background-job form of the direct demo upload: the endpoint buffers the multipart file to a server-generated
/// temp file (<see cref="UploadedDemoFiles.NewTempPath"/>) and this request analyses and then deletes it.
/// <paramref name="FileName"/> is the browser's original name, used to recognise a FACEIT match.</summary>
public record AnalyzeUploadedDemoCommand(string FilePath, string? FileName = null) : IRequest<ErrorOr<AnalyzeDemoResultDto>>;
