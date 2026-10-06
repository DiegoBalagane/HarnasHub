#region Usings

using ErrorOr;
using HarnasHub.Application.Features.Results.AnalyzeDemo;
using HarnasHub.Application.Features.Results.Shared;
using MediatR;
using Microsoft.Extensions.Logging;

#endregion

namespace HarnasHub.Application.Features.Results.AnalyzeUploadedDemo;

/// <summary>Handles <see cref="AnalyzeUploadedDemoCommand"/> by delegating to <see cref="AnalyzeDemoCommand"/> (so every
/// upload path parses identically) and always deleting the temp file afterwards.</summary>
public class AnalyzeUploadedDemoHandler(ISender sender, ILogger<AnalyzeUploadedDemoHandler> logger)
	: IRequestHandler<AnalyzeUploadedDemoCommand, ErrorOr<AnalyzeDemoResultDto>>
{
	#region Public Methods

	/// <inheritdoc />
	public async Task<ErrorOr<AnalyzeDemoResultDto>> Handle(AnalyzeUploadedDemoCommand request, CancellationToken cancellationToken)
	{
		try
		{
			await using var stream = new FileStream(request.FilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
			return await sender.Send(new AnalyzeDemoCommand(stream, request.FileName), cancellationToken);
		}
		catch (IOException ex)
		{
			logger.LogWarning(ex, "Nie znaleziono tymczasowego pliku wgranej demki {Path}", request.FilePath);
			return ResultErrors.InvalidDemoFile;
		}
		finally
		{
			UploadedDemoFiles.TryDelete(request.FilePath);
		}
	}

	#endregion
}
