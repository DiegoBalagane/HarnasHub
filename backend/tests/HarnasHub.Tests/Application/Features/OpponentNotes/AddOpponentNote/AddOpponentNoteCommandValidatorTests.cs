using FluentValidation.TestHelper;
using HarnasHub.Application.Features.OpponentNotes.AddOpponentNote;
using Xunit;

namespace HarnasHub.Tests.Application.Features.OpponentNotes.AddOpponentNote;

public class AddOpponentNoteCommandValidatorTests
{
	#region Private Fields

	private readonly AddOpponentNoteCommandValidator _validator = new();

	#endregion

	#region Public Methods

	[Fact]
	public void Should_have_error_when_content_is_empty()
	{
		var command = new AddOpponentNoteCommand("Team X", string.Empty, null);

		var result = _validator.TestValidate(command);

		result.ShouldHaveValidationErrorFor(x => x.Content);
	}

	[Fact]
	public void Should_have_error_when_material_url_is_malformed()
	{
		var command = new AddOpponentNoteCommand("Team X", "Grają agresywnie na T", "not-a-url");

		var result = _validator.TestValidate(command);

		result.ShouldHaveValidationErrorFor(x => x.MaterialUrl);
	}

	[Fact]
	public void Should_not_have_errors_for_a_valid_command()
	{
		var command = new AddOpponentNoteCommand("Team X", "Grają agresywnie na T", "https://example.com/demo.dem");

		var result = _validator.TestValidate(command);

		result.ShouldNotHaveAnyValidationErrors();
	}

	#endregion
}
