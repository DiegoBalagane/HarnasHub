using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.Results.Shared;
using MediatR;

namespace HarnasHub.Application.Features.Results.PresignDemoUpload;

/// <summary>Handles <see cref="PresignDemoUploadCommand"/> by asking <see cref="IFileStorage"/> for a presigned PUT URL.</summary>
public class PresignDemoUploadHandler(IFileStorage fileStorage) : IRequestHandler<PresignDemoUploadCommand, ErrorOr<PresignDemoUploadResultDto>>
{
	#region Public Methods

	public async Task<ErrorOr<PresignDemoUploadResultDto>> Handle(PresignDemoUploadCommand request, CancellationToken cancellationToken)
	{
		if (!fileStorage.IsConfigured)
		{
			return ResultErrors.StorageNotConfigured;
		}

		var upload = await fileStorage.CreatePresignedUploadAsync("demos", TimeSpan.FromMinutes(20), cancellationToken);
		return new PresignDemoUploadResultDto(upload.UploadUrl, upload.ObjectKey);
	}

	#endregion
}
