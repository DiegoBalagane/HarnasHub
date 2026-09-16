using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;

namespace HarnasHub.Application.Features.Availability.Shared;

/// <summary>Effective availability of one member on one day, after vacations have been applied.</summary>
public readonly record struct EffectiveAvailability(
	string Status,
	TimeOnly? From,
	TimeOnly? To,
	bool IsVacation,
	string? Note);

/// <summary>Resolves the effective day availability shared by the weekly grid and the dashboard.</summary>
public static class EffectiveAvailabilityCalculator
{
	#region Public Fields

	/// <summary>Status reported when a member has neither a declaration nor a vacation for the day.</summary>
	public const string NotSetStatus = "NotSet";

	#endregion

	#region Public Methods

	/// <summary>Returns the member's status for one day: a vacation wins, then a declaration, otherwise "NotSet".</summary>
	public static EffectiveAvailability Resolve(
		Guid userId,
		DateOnly date,
		IReadOnlyDictionary<(Guid UserId, DateOnly Date), PlayerAvailabilityDay> declaredByUserAndDate,
		ILookup<Guid, Vacation> vacationsByUser)
	{
		var vacation = vacationsByUser[userId]
			.FirstOrDefault(candidate => candidate.StartDate <= date && candidate.EndDate >= date);

		if (vacation is not null)
		{
			return new EffectiveAvailability(nameof(DayAvailabilityStatus.Off), null, null, true, vacation.Reason);
		}

		if (declaredByUserAndDate.TryGetValue((userId, date), out var declared))
		{
			return new EffectiveAvailability(
				declared.Status.ToString(),
				declared.AvailableFromLocal,
				declared.AvailableToLocal,
				false,
				declared.Note);
		}

		return new EffectiveAvailability(NotSetStatus, null, null, false, null);
	}

	#endregion
}
