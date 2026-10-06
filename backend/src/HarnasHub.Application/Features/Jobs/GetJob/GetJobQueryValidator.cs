#region Usings

using FluentValidation;

#endregion

namespace HarnasHub.Application.Features.Jobs.GetJob;

/// <summary>Validation rules for <see cref="GetJobQuery"/>.</summary>
public class GetJobQueryValidator : AbstractValidator<GetJobQuery>
{
	#region Constructors

	/// <summary>Requires a job id.</summary>
	public GetJobQueryValidator()
	{
		RuleFor(x => x.JobId).NotEmpty().WithMessage("Brak identyfikatora zadania.");
	}

	#endregion
}
