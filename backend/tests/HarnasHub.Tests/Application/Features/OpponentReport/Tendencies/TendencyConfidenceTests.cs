#region Usings

using HarnasHub.Application.Features.OpponentReport.Tendencies;
using Xunit;

#endregion

namespace HarnasHub.Tests.Application.Features.OpponentReport.Tendencies;

public class TendencyConfidenceTests
{
	#region Public Methods

	[Theory]
	[InlineData(0, "Low")]
	[InlineData(9, "Low")]
	[InlineData(10, "Medium")]
	[InlineData(24, "Medium")]
	[InlineData(25, "High")]
	public void Should_grade_confidence_by_rounds(int rounds, string expected)
	{
		Assert.Equal(expected, TendencyConfidence.For(rounds));
	}

	[Fact]
	public void Should_compute_percent_with_one_decimal_and_zero_for_an_empty_sample()
	{
		Assert.Equal(33.3, TendencyConfidence.Percent(1, 3));
		Assert.Equal(0, TendencyConfidence.Percent(1, 0));
	}

	#endregion
}
