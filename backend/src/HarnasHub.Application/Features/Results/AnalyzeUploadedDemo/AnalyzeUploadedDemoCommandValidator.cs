#region Usings

using FluentValidation;

#endregion

namespace HarnasHub.Application.Features.Results.AnalyzeUploadedDemo;

/// <summary>Validation rules for <see cref="AnalyzeUploadedDemoCommand"/>.</summary>
public class AnalyzeUploadedDemoCommandValidator : AbstractValidator<AnalyzeUploadedDemoCommand>
{
	#region Constructors

	/// <summary>Requires a server-generated upload temp path and caps the original file name.</summary>
	public AnalyzeUploadedDemoCommandValidator()
	{
		RuleFor(x => x.FilePath)
			.Must(UploadedDemoFiles.IsUploadTempPath).WithMessage("Nieprawidłowy plik demki.");

		RuleFor(x => x.FileName)
			.MaximumLength(260).WithMessage("Nazwa pliku demki może mieć maksymalnie 260 znaków.");
	}

	#endregion
}
