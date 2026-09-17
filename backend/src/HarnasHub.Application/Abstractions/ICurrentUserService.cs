namespace HarnasHub.Application.Abstractions;

/// <summary>Identifies the user making the current request.</summary>
public interface ICurrentUserService
{
	Guid UserId { get; }

	/// <summary>The caller's access level as a claim string ("Guest"/"Player"/"Manager") — never "Coach", which is tracked separately by <see cref="IsCoach"/>.</summary>
	string Role { get; }

	/// <summary>Whether the caller is tagged as the team's coach, independent of their access level.</summary>
	bool IsCoach { get; }
}
