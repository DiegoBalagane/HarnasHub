using FluentValidation.TestHelper;
using HarnasHub.Application.Features.TeamInfo.CreateTeamInfoEntry;
using HarnasHub.Application.Features.TeamInfo.ReorderTeamInfoEntries;
using HarnasHub.Application.Features.TeamInfo.UpdateTeamInfoEntry;
using Xunit;

namespace HarnasHub.Tests.Application.Features.TeamInfo;

public class TeamInfoValidatorTests
{
	#region Public Methods

	[Fact]
	public void Create_should_accept_a_valid_command()
	{
		var result = new CreateTeamInfoEntryCommandValidator().TestValidate(new CreateTeamInfoEntryCommand("Discord", "Serwer", "https://discord.gg/x", false));

		result.ShouldNotHaveAnyValidationErrors();
	}

	[Fact]
	public void Create_should_reject_empty_and_too_long_fields()
	{
		var validator = new CreateTeamInfoEntryCommandValidator();

		var empty = validator.TestValidate(new CreateTeamInfoEntryCommand(" ", "", "", false));
		var tooLong = validator.TestValidate(new CreateTeamInfoEntryCommand(new string('a', 51), new string('a', 101), new string('a', 2001), false));

		empty.ShouldHaveValidationErrorFor(x => x.Category);
		empty.ShouldHaveValidationErrorFor(x => x.Title);
		empty.ShouldHaveValidationErrorFor(x => x.Value);
		tooLong.ShouldHaveValidationErrorFor(x => x.Category).WithErrorMessage("Kategoria może mieć maksymalnie 50 znaków.");
		tooLong.ShouldHaveValidationErrorFor(x => x.Title);
		tooLong.ShouldHaveValidationErrorFor(x => x.Value);
	}

	[Fact]
	public void Update_should_require_an_id_and_the_same_field_limits()
	{
		var result = new UpdateTeamInfoEntryCommandValidator().TestValidate(new UpdateTeamInfoEntryCommand(Guid.Empty, "", "", "", false));

		result.ShouldHaveValidationErrorFor(x => x.Id);
		result.ShouldHaveValidationErrorFor(x => x.Category);
		result.ShouldHaveValidationErrorFor(x => x.Title);
		result.ShouldHaveValidationErrorFor(x => x.Value);
	}

	[Fact]
	public void Reorder_should_reject_empty_and_duplicated_ids()
	{
		var validator = new ReorderTeamInfoEntriesCommandValidator();
		var id = Guid.NewGuid();

		validator.TestValidate(new ReorderTeamInfoEntriesCommand([])).ShouldHaveValidationErrorFor(x => x.OrderedIds);
		validator.TestValidate(new ReorderTeamInfoEntriesCommand([id, id])).ShouldHaveValidationErrorFor(x => x.OrderedIds);
		validator.TestValidate(new ReorderTeamInfoEntriesCommand([id, Guid.NewGuid()])).ShouldNotHaveAnyValidationErrors();
	}

	#endregion
}
