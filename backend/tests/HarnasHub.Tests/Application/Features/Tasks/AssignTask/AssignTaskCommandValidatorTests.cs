using FluentValidation.TestHelper;
using HarnasHub.Application.Features.Tasks.AssignTask;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Tasks.AssignTask;

public class AssignTaskCommandValidatorTests
{
	#region Private Fields

	private readonly AssignTaskCommandValidator _validator = new();

	#endregion

	#region Public Methods

	[Fact]
	public void Should_have_error_when_title_is_empty()
	{
		var command = new AssignTaskCommand(string.Empty, null, Guid.NewGuid(), null);

		var result = _validator.TestValidate(command);

		result.ShouldHaveValidationErrorFor(x => x.Title);
	}

	[Fact]
	public void Should_have_error_when_assignee_is_empty()
	{
		var command = new AssignTaskCommand("Obejrzyj demo", null, Guid.Empty, null);

		var result = _validator.TestValidate(command);

		result.ShouldHaveValidationErrorFor(x => x.AssignedToUserId);
	}

	[Fact]
	public void Should_not_have_errors_for_a_valid_command()
	{
		var command = new AssignTaskCommand("Obejrzyj demo", "Mecz z drużyną X", Guid.NewGuid(), DateTime.UtcNow.AddDays(2));

		var result = _validator.TestValidate(command);

		result.ShouldNotHaveAnyValidationErrors();
	}

	#endregion
}
