#region Usings

using ErrorOr;
using HarnasHub.Application.Features.Results.AnalyzeUploadedDemo;
using HarnasHub.Application.Features.Results.Shared;
using HarnasHub.Tests.Common;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

#endregion

namespace HarnasHub.Tests.Application.Features.Results.AnalyzeUploadedDemo;

public class AnalyzeUploadedDemoHandlerTests
{
	#region Public Methods

	[Fact]
	public async Task Should_delegate_the_analysis_and_delete_the_temp_file()
	{
		var path = UploadedDemoFiles.NewTempPath();
		await File.WriteAllBytesAsync(path, [1, 2, 3]);
		var preview = new AnalyzeDemoResultDto(1, "Mirage", new([], [], 1, 0), new([], [], 0, 1), null, []);
		var handler = new AnalyzeUploadedDemoHandler(
			new TestSender<ErrorOr<AnalyzeDemoResultDto>>(preview), NullLogger<AnalyzeUploadedDemoHandler>.Instance);

		var result = await handler.Handle(new AnalyzeUploadedDemoCommand(path, "scrim.dem"), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Same(preview, result.Value);
		Assert.False(File.Exists(path));
	}

	[Fact]
	public async Task Should_return_invalid_demo_when_the_temp_file_is_gone()
	{
		var handler = new AnalyzeUploadedDemoHandler(
			new TestSender<ErrorOr<AnalyzeDemoResultDto>>(ResultErrors.InvalidDemoFile), NullLogger<AnalyzeUploadedDemoHandler>.Instance);

		var result = await handler.Handle(new AnalyzeUploadedDemoCommand(UploadedDemoFiles.NewTempPath()), CancellationToken.None);

		Assert.Equal(ResultErrors.InvalidDemoFile, result.FirstError);
	}

	[Fact]
	public void Validator_should_accept_only_server_generated_temp_paths()
	{
		var validator = new AnalyzeUploadedDemoCommandValidator();

		Assert.True(validator.Validate(new AnalyzeUploadedDemoCommand(UploadedDemoFiles.NewTempPath(), "a.dem")).IsValid);
		Assert.False(validator.Validate(new AnalyzeUploadedDemoCommand(Path.Combine(Path.GetTempPath(), "other.dem"))).IsValid);
		Assert.False(validator.Validate(new AnalyzeUploadedDemoCommand("/etc/harnashub-demo-0123456789abcdef0123456789abcdef.dem")).IsValid);
		Assert.False(validator.Validate(new AnalyzeUploadedDemoCommand(UploadedDemoFiles.NewTempPath(), new string('a', 261))).IsValid);
	}

	#endregion
}
