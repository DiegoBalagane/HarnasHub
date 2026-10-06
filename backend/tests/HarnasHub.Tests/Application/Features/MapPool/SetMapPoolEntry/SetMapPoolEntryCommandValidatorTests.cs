using FluentValidation.TestHelper;
using HarnasHub.Application.Features.MapPool.SetMapPoolEntry;
using HarnasHub.Core.Enums;
using Xunit;

namespace HarnasHub.Tests.Application.Features.MapPool.SetMapPoolEntry;

public class SetMapPoolEntryCommandValidatorTests
{
	#region Private Fields

	private readonly SetMapPoolEntryCommandValidator _validator = new();

	#endregion

	#region Public Methods

	[Fact]
	public void Should_have_error_for_an_unknown_map()
	{
		var result = _validator.TestValidate(new SetMapPoolEntryCommand((MapName)99, MapPoolStatus.Core, null));

		result.ShouldHaveValidationErrorFor(x => x.MapName);
	}

	[Fact]
	public void Should_have_error_for_an_unknown_status()
	{
		var result = _validator.TestValidate(new SetMapPoolEntryCommand(MapName.Mirage, (MapPoolStatus)99, null));

		result.ShouldHaveValidationErrorFor(x => x.Status);
	}

	[Fact]
	public void Should_have_error_when_note_is_too_long()
	{
		var result = _validator.TestValidate(new SetMapPoolEntryCommand(MapName.Mirage, MapPoolStatus.Core, new string('x', 301)));

		result.ShouldHaveValidationErrorFor(x => x.Note);
	}

	[Fact]
	public void Should_accept_a_cleared_status()
	{
		var result = _validator.TestValidate(new SetMapPoolEntryCommand(MapName.Mirage, null, null));

		result.ShouldNotHaveAnyValidationErrors();
	}

	#endregion
}
