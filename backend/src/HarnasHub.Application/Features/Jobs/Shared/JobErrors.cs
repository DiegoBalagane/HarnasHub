#region Usings

using ErrorOr;

#endregion

namespace HarnasHub.Application.Features.Jobs.Shared;

/// <summary>Domain errors of the background job feature.</summary>
public static class JobErrors
{
	#region Public Properties

	/// <summary>No job with this id (or it was already cleaned up).</summary>
	public static Error NotFound => Error.NotFound("Jobs.NotFound", "Nie znaleziono zadania — mogło już wygasnąć.");

	/// <summary>The caller neither started the job nor is Coach/Manager.</summary>
	public static Error Forbidden => Error.Forbidden("Jobs.Forbidden", "Nie masz dostępu do tego zadania.");

	/// <summary>A request type was sent as a job without being registered in <c>JobRegistry</c> (programming error).</summary>
	public static Error UnknownKind => Error.Failure("Jobs.UnknownKind", "Ta operacja nie może zostać uruchomiona w tle.");

	#endregion
}
