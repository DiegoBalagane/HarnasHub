using FluentValidation.TestHelper;
using HarnasHub.Application.Features.OpponentReport.GetOpponentReport;
using Xunit;

namespace HarnasHub.Tests.Application.Features.OpponentReport.GetOpponentReport;

public class GetOpponentReportQueryValidatorTests
{
	#region Private Fields

	private readonly GetOpponentReportQueryValidator _validator = new();

	#endregion

	#region Public Methods

	[Fact]
	public void Should_have_error_when_name_is_empty()
	{
		_validator.TestValidate(new GetOpponentReportQuery(" ")).ShouldHaveValidationErrorFor(x => x.Name);
	}

	[Fact]
	public void Should_not_have_errors_for_a_valid_name()
	{
		_validator.TestValidate(new GetOpponentReportQuery("Team X")).ShouldNotHaveAnyValidationErrors();
	}

	#endregion
}
