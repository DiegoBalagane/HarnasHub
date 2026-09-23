using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.AnalysisBoards.Shared;
using MediatR;

namespace HarnasHub.Application.Features.AnalysisBoards.PresignBoardImageUpload;

/// <summary>Handles <see cref="PresignBoardImageUploadCommand"/> by asking <see cref="IFileStorage"/> for a presigned PUT URL.</summary>
public class PresignBoardImageUploadHandler(IFileStorage fileStorage)
	: IRequestHandler<PresignBoardImageUploadCommand, ErrorOr<PresignBoardImageUploadResultDto>>
{
	#region Public Methods

	public async Task<ErrorOr<PresignBoardImageUploadResultDto>> Handle(PresignBoardImageUploadCommand request, CancellationToken cancellationToken)
	{
		if (!fileStorage.IsConfigured)
		{
			return AnalysisBoardErrors.StorageNotConfigured;
		}

		var upload = await fileStorage.CreatePresignedUploadAsync("analysis-boards", TimeSpan.FromMinutes(10), cancellationToken);
		return new PresignBoardImageUploadResultDto(upload.UploadUrl, upload.ObjectKey);
	}

	#endregion
}
