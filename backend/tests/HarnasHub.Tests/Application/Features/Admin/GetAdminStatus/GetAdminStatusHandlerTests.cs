using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.Admin.GetAdminStatus;
using HarnasHub.Tests.Common;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Admin.GetAdminStatus;

public class GetAdminStatusHandlerTests
{
	#region Public Methods

	[Fact]
	public async Task Should_report_everything_configured()
	{
		var handler = CreateHandler(true, true, true, true, true);

		var result = await handler.Handle(new GetAdminStatusQuery(), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Equal(new AdminStatusDto(true, true, true, true, true), result.Value);
	}

	[Fact]
	public async Task Should_report_everything_missing()
	{
		var handler = CreateHandler(false, false, false, false, false);

		var result = await handler.Handle(new GetAdminStatusQuery(), CancellationToken.None);

		Assert.Equal(new AdminStatusDto(false, false, false, false, false), result.Value);
	}

	[Fact]
	public async Task Should_map_each_flag_to_its_own_integration()
	{
		var handler = CreateHandler(true, false, true, false, true);

		var result = await handler.Handle(new GetAdminStatusQuery(), CancellationToken.None);

		Assert.True(result.Value.FaceitApiKeyConfigured);
		Assert.False(result.Value.FaceitDownloadsTokenConfigured);
		Assert.True(result.Value.S3Configured);
		Assert.False(result.Value.DiscordWebhookConfigured);
		Assert.True(result.Value.FrontendBaseUrlConfigured);
	}

	#endregion

	#region Private Methods

	private static GetAdminStatusHandler CreateHandler(bool faceit, bool downloads, bool s3, bool discord, bool frontend) =>
		new(
			new TestFaceitClient(faceit),
			new TestFaceitDemoDownloader(downloads),
			new TestFileStorage(s3),
			new StubIntegrationSettings(discord, frontend));

	#endregion

	private sealed class StubIntegrationSettings(bool discord, bool frontend) : IIntegrationSettings
	{
		public bool IsDiscordWebhookConfigured { get; } = discord;

		public bool IsFrontendBaseUrlConfigured { get; } = frontend;
	}
}
