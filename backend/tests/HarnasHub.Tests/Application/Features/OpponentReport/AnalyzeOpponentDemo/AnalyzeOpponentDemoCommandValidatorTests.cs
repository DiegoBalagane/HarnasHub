#region Usings

using HarnasHub.Application.Features.OpponentReport.AnalyzeOpponentDemo;
using Xunit;

#endregion

namespace HarnasHub.Tests.Application.Features.OpponentReport.AnalyzeOpponentDemo;

public class AnalyzeOpponentDemoCommandValidatorTests
{
	#region Public Methods

	[Fact]
	public void Should_accept_a_presigned_demo_key()
	{
		Assert.True(new AnalyzeOpponentDemoCommandValidator().Validate(new AnalyzeOpponentDemoCommand("Team X", "demos/0123456789abcdef0123456789abcdef")).IsValid);
	}

	[Theory]
	[InlineData("", "demos/0123456789abcdef0123456789abcdef")]
	[InlineData("Team X", "opponents/team-x/abc.json.gz")]
	[InlineData("Team X", "")]
	public void Should_reject_a_missing_name_or_a_foreign_key(string name, string key)
	{
		Assert.False(new AnalyzeOpponentDemoCommandValidator().Validate(new AnalyzeOpponentDemoCommand(name, key)).IsValid);
	}

	[Fact]
	public void Should_reject_a_too_long_name()
	{
		Assert.False(new AnalyzeOpponentDemoCommandValidator().Validate(new AnalyzeOpponentDemoCommand(new string('x', 101), "demos/0123456789abcdef0123456789abcdef")).IsValid);
	}

	#endregion
}
