using FluentValidation.TestHelper;
using HarnasHub.Application.Features.OpponentNotes.UpdateOpponentNote;
using Xunit;

namespace HarnasHub.Tests.Application.Features.OpponentNotes.UpdateOpponentNote;

public class UpdateOpponentNoteCommandValidatorTests
{
	#region Private Fields

	private readonly UpdateOpponentNoteCommandValidator _validator = new();

	#endregion

	#region Public Methods

	[Fact]
	public void Should_have_error_when_note_id_is_empty()
	{
		var command = new UpdateOpponentNoteCommand(Guid.Empty, "Team X", "Treść", null);

		var result = _validator.TestValidate(command);

		result.ShouldHaveValidationErrorFor(x => x.NoteId);
	}

	[Fact]
	public void Should_have_error_when_opponent_name_is_empty()
	{
		var command = new UpdateOpponentNoteCommand(Guid.NewGuid(), " ", "Treść", null);

		var result = _validator.TestValidate(command);

		result.ShouldHaveValidationErrorFor(x => x.OpponentName);
	}

	[Fact]
	public void Should_have_error_when_material_url_is_malformed()
	{
		var command = new UpdateOpponentNoteCommand(Guid.NewGuid(), "Team X", "Treść", "not-a-url");

		var result = _validator.TestValidate(command);

		result.ShouldHaveValidationErrorFor(x => x.MaterialUrl);
	}

	[Fact]
	public void Should_not_have_errors_for_a_valid_command()
	{
		var command = new UpdateOpponentNoteCommand(Guid.NewGuid(), "Team X", "Treść", "https://example.com/demo");

		var result = _validator.TestValidate(command);

		result.ShouldNotHaveAnyValidationErrors();
	}

	#endregion
}
