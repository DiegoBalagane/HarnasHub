#region Usings

using FluentValidation;
using HarnasHub.Application.Features.OpponentReport.LinkOpponentFaceit;

#endregion

namespace HarnasHub.Application.Features.OpponentReport.LinkAndSyncOpponentFaceit;

/// <summary>Validation rules for <see cref="LinkAndSyncOpponentFaceitCommand"/> — exactly those of the plain link.</summary>
public class LinkAndSyncOpponentFaceitCommandValidator : AbstractValidator<LinkAndSyncOpponentFaceitCommand>
{
	#region Private Fields

	private static readonly LinkOpponentFaceitCommandValidator LinkValidator = new();

	#endregion

	#region Constructors

	/// <summary>Delegates to <see cref="LinkOpponentFaceitCommandValidator"/> so both entry points can never drift apart.</summary>
	public LinkAndSyncOpponentFaceitCommandValidator()
	{
		RuleFor(x => x).Custom((command, context) =>
		{
			var result = LinkValidator.Validate(new LinkOpponentFaceitCommand(command.OpponentName, command.Source));
			foreach (var failure in result.Errors)
			{
				context.AddFailure(failure);
			}
		});
	}

	#endregion
}
