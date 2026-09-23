using FluentValidation.TestHelper;
using HarnasHub.Application.Features.TrainingMaterials.AddMaterial;
using HarnasHub.Core.Enums;
using Xunit;

namespace HarnasHub.Tests.Application.Features.TrainingMaterials.AddMaterial;

public class AddMaterialCommandValidatorTests
{
	#region Private Fields

	private readonly AddMaterialCommandValidator _validator = new();

	#endregion

	#region Public Methods

	[Fact]
	public void Should_have_error_when_url_is_malformed()
	{
		var command = new AddMaterialCommand("Analiza VOD", "not-a-url", MaterialCategory.VodReview, null);

		var result = _validator.TestValidate(command);

		result.ShouldHaveValidationErrorFor(x => x.Url);
	}

	[Fact]
	public void Should_not_have_errors_for_a_valid_command()
	{
		var command = new AddMaterialCommand("Analiza VOD", "https://youtube.com/watch?v=abc", MaterialCategory.VodReview, "Rozbiór ostatniego meczu");

		var result = _validator.TestValidate(command);

		result.ShouldNotHaveAnyValidationErrors();
	}

	#endregion
}
