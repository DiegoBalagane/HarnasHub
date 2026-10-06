#region Usings

using HarnasHub.Application.Features.Results.AnalyzeDemoFromStorage;
using Xunit;

#endregion

namespace HarnasHub.Tests.Application.Features.Results.AnalyzeDemoFromStorage;

public class AnalyzeDemoFromStorageCommandValidatorTests
{
	#region Public Methods

	[Theory]
	[InlineData("demos/0123456789abcdef0123456789abcdef", "1-abc.dem", true)]
	[InlineData("demos/0123456789abcdef0123456789abcdef", null, true)]
	[InlineData("timelines/pending/0123456789abcdef0123456789abcdef.json.gz", null, false)]
	[InlineData("", null, false)]
	public void Should_accept_only_presigned_demo_keys(string objectKey, string? fileName, bool valid)
	{
		Assert.Equal(valid, new AnalyzeDemoFromStorageCommandValidator().Validate(new AnalyzeDemoFromStorageCommand(objectKey, fileName)).IsValid);
	}

	#endregion
}
