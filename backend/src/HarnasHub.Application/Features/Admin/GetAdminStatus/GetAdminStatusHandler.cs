using ErrorOr;
using HarnasHub.Application.Abstractions;
using MediatR;

namespace HarnasHub.Application.Features.Admin.GetAdminStatus;

/// <summary>Handles <see cref="GetAdminStatusQuery"/> by collecting configuration flags from the integration abstractions.</summary>
public class GetAdminStatusHandler(
	IFaceitClient faceitClient,
	IFaceitDemoDownloader demoDownloader,
	IFileStorage fileStorage,
	IIntegrationSettings integrationSettings)
	: IRequestHandler<GetAdminStatusQuery, ErrorOr<AdminStatusDto>>
{
	#region Public Methods

	public Task<ErrorOr<AdminStatusDto>> Handle(GetAdminStatusQuery request, CancellationToken cancellationToken)
	{
		ErrorOr<AdminStatusDto> status = new AdminStatusDto(
			faceitClient.IsConfigured,
			demoDownloader.IsConfigured,
			fileStorage.IsConfigured,
			integrationSettings.IsDiscordWebhookConfigured,
			integrationSettings.IsFrontendBaseUrlConfigured);

		return Task.FromResult(status);
	}

	#endregion
}
